# Summary

An experimental project to implement an OpenTelemetry Metrics API and SDK.

Our goals are:

- To discover/design a simpler approach to Metrics API
- To explore different SDK implementation based on Metrics API design
- To experiment with alternative designs/models/ideas for evaluation
- To complement existing/current .NET runtime design
- To maintain zero/minimal differences between OpenTelemetry specification and .NET runtime
- To use this project as an informed starting base for discussion with wider OpenTelemetry community

## Principles

- .NET provides a way to pass datapoints (un-opinionated) in a Time Series.  With
optional metadata / hints for downstream interpretation.

- .NET may implement and deliver counters (derived from .NET MetricBase) with it's
own Semantic. (i.e. Counter with Add(), ElapsedDuration as a IDisposable, etc...)

- OTel may implement and deliver counters (derived from .NET MetricBase) with it's
own Semantic. (i.e. UpCounter with Inc(), ValueRecorder with Record(), etc...)

- MetricBase will report datapoints into the .NET MetricSource interface
(EventSource/EventListener model).  OTel SDK will attached as a MetricListeners.

- SDKs will be opinionated!  Thus, datapoints will be interpreted based
on it's counter "type" for aggregators, processors, exporters, etc...

  - For unknown "type", it is treated as a pass-thru / standard Time Series
  (i.e. as Gauge)

- Exporters will need to "normalize" datapoints into "un-opinionated" OTLP

# API questions

## What should 'Instrument' concepts be named?

By 'Instrument' I am refering to the types that have the Record(), Add(), Set(), etc API that .NET developers
would invoke to record a value and send it to the SDK. There are multiple potential names to decide on here:
1. There is the general concept, what OpenTelemetry calls 'Instrument'. Some other potential terms are
 'Meter', 'Metric', 'Measure', and 'Counter'.
2. There are specific types of instrument, usually classified by how the numbers that are provided through the 
API are aggregated. For example OpenTelemetry calls some of these ValueRecorder or UpDownSumObserver. Other 
libraries use terms like Gauge, Counter, and Histogram.

## What instruments should the API support?

OpenTelemetry proposes Counter, UpDownCounter, ValueRecorder, SumObserver, UpDownSumObserver, and ValueObserver.
Research into other libraries suggests these choices, both in terms of naming and functionality appear mostly
novel. Based on the spec description this set of six appears to be determined based on defining a taxonomy of
'synchronous vs asynchronous', 'adding vs. grouping', and 'monotonic vs. non-monotonic' as a sub-category of
'adding'. This created six combinations which were then named. While taxonomies are certainly orderly I am 
skeptical how well most developers will related their goals to this particular breakdown. Other metric APIs that
have considerable real world usage appear to have selected other patterns, likely driven by customer feedback.

## Do we need dedicated asynchronous instruments?

The asynchronous instruments are designed to offer callback APIs rather than standard push based APIs. These
callbacks are invoked at whatever frequency aggregated metric data is exported to consumers. However design-wise
it appears unnecessarily complex to define a complete parallel set of instruments with individual callbacks when
we could define a single callback that occurs prior to aggregation. For example OT's existing async API might do this:

````
func (s *server) registerObservers(.Context) {
     s.observer1 = s.meter.NewInt64SumObserver(
         "service_load_factor",
          metric.WithCallback(func(result metric.Float64ObserverResult) {
             for _, listener := range s.listeners {
                 result.Observe(
                     s.loadFactor(),
                     kv.String("name", server.name),
                     kv.String("port", listener.port),
                 )
             }
          }),
          metric.WithDescription("The load factor use for load balancing purposes"),
    )
}
````

An alternative API could be:
````C#
// This reuses the same instrument types that are used for synchronous calls, 
// whatever that happens to be
Metric observer1 = new Metric("service_load_factor");

// We might need to finesse what object or scope this callback is registered
// at but for simplicity here assume it is a global static.
Metrics.BeforeExport += (sender,args) => 
{
    observer1.Set(s.loadFactor(), 
                  new Label("name", server.name),
                  new Label("port", listener.port));
    // any number of additional metrics could also be set here if desired
}
````

One interesting wrinkle is that as OT defines the async adding instruments as having an Observe() API. This API
takes the pre-aggregated sum whereas the synchronous version took the increment. In short the async adding instruments
don't actually add. The only reason they are considered adding instruments is to provide a hint that if further
aggregation were to occur upstream, for example to aggregate samples from multiple machines or to aggregate into larger
time intervals then addition would probably be the recommended aggregation function at that point. We would need to
decide if and how we wanted to represent that on the synchronous instruments.


## Is batching needed?

I don't recall seeing a batching capability on any of the other metric APIs I looked at. What scenario justifies
including it in OpenTelemetry?

Batching is one spot in the API where we pay a performance penalty if we don't have a metric grouping concept. As
defined in the OT APIs batches only include measurements on the same Meter. This lets the API completely ignore the
batch operation if the SDK isn't subscribed to that Meter. If there were no Meter (or alternative metric grouping
mechanism) then batches would be allowed to contain any metric and there would be no way to know a-priori that it
was safe to ignore the batch. Somewhere in the API or SDK some processing would need to record that the batch had
started and stopped even if none of the metrics set inside the batch were being subscribed to.


## Should metric types be represented as interfaces or classes in .NET?

Proposed answer: Use a class

Rationale:
OpenTelemetry often uses [Service Provider Interface patterns](https://en.wikipedia.org/wiki/Service_provider_interface)
where the API defines interfaces and a registration function for a factory that will produce implementations of that
interface. This allows the SDK implementation to take complete control of the interface implementation. However our .NET
runtime API goal is to create a pub-sub model where OpenTelemetry SDK is one potential subscriber. This should create an
API surface that follows the OT spec but does not give any single subscriber total control over the implementation of that
API. In particular no subscriber should be able to prevent other subscribers from receiving data by implementing all the
APIs as no-ops. This means if there is some Metric.Record(double measument) function, .NET needs to provide the pub-sub
implementation that dispatches the value to each subscriber:
````C#
public void Record(double measurement)
{
    foreach(MetricListener l in _listeners)
    {
        l.MeasurementRecorded(this, measurement);
    }
}
````
There is no particular value to defining metrics as an interface type because by design we don't want to provide an
option for other components to implement that functionality differently. Instead we expect components that would
have implemented the interface to subscribe as a listener and implement their custom functionality in the callbacks.
We should make sure the combination of the callback interface and property getters on the Metric types are rich
enough for expected usage by the OT SDK and for testing mocks.

In theory we could still have an interface even with a single runtime implementation, but in this case both [.NET API
usability guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) and performance considerations
suggest a class is better. On usability, all else being equal, .NET recommends using concrete types that a developer can
create via the 'new' operator. Largely this is to minimize the number of concepts or abstractions that a developer needs
to understand to use the API. For performance calls through interfaces are harder for compilers to devirtualize and
inline which can create small (nanosecond scale) penalties for using an interface. Some metric scenarios are expected
to be performance critical so there is no reason for us to potentially worsen performance if we get no benefit from it.

## Do users create a new Metric directly with 'new' or is the creation indirected through a factory (or both)? 

#### 'new' option
````C#
Counter c = new Counter("hats-sold");
Gauge g = new Gauge("temperature");
````

#### factory option
There are different reasons a factory might be useful and the naming would probably change depending
on why we wanted it. Current OT spec uses factories to provide an extensibility point to substitute
different implementations of the metric objects. Another use of a factory is to provide an object that
represents a common set of metadata or configuration information that can be shared by a group of metrics.
A 3rd use of a factory is to provide a grouping mechanism so that metric data subscribers can register
their interest in a group of metrics rather than selecting individual metrics. A 4th use is to allow
metric creation functions to potentially reuse existing objects, such as deduplicating based on name or
recycling objects with an allocation pool. A 5th use is to match past expectations for users that used
other factory based metric or telemetry APIs such as Prometheus.NET, Micrometer, 
````C#
// the arguments and likely type name depend on why we are using a factory
MetricFactory factory = new MetricFactory(...); 
Counter c = factory.CreateCounter("hats-sold");
Gauge g = factory.CreateGauge("temperature");
````

#### both 'new' and factory option
This option only works for a subset of the reasons we might want a factory. It does not work if the
factory was intended to pick the concrete type that instantiates the metric or if the factory is
intended to reuse objects, the other uses above are fine.
````C#
// the arguments and likely type name depend on why we are using a factory
MetricFactory factory = new MetricFactory(...); 

// dev could write this
Counter c = factory.CreateCounter("hats-sold");
Gauge g = factory.CreateGauge("temperature");

// OR this
Counter c = new Counter(factory, "hats-sold");
Guage g = new Gauge(factory, "temperature");

// OR potentially still this if there is a default global factory
Counter c = new Counter("hats-sold");
Guage g = new Gauge("temperature");
````

### Design discussion

- Per the .NET design guidelines, 'new' this is likely the most easily understood and discovered mechanism
   (but that doesn't mean it trumps any other concern)
- Its hard to determine if we should use a factory until we determine what value are we trying to get from
   the factory.
  - I don't think we should be using the factory to do what OTel uses its 'Provider' types for, allowing
    a 3rd party to replace the core implementation of metric. .NET has an interest in ensuring that multiple
    data consumers each have access to measurement data which means there needs to be a .NET provided impl
    that at least iterates over the list of consumers and dispatches the value to each of them. .NET also
    wants to support scenarios that where the app developer may not have used OpenTelemetry inside their
    application but still wants to observe metric data with Visual Studio or SDK tools. This means that .NET
    runtime itself may be a data consumer forwarding it to out-of-process tools.
  - The .NET runtime has no apparent need to return pre-existing objects from the factory. We don't expect
    metric creation operations or disposal to occur often enough to be have meaningful performance impact. We
    also have no clear scenario need to de-duplicate. Even if metrics had identical names and other metadata
    the runtime isn't in position to know that merging the collected data is appropriate. Consumers can still
    make the choice to dedupe regardless of whether the runtime dedupes the object references.
  - Grouping metrics by factory for the purpose of making subscription decisions in aggregate (like what we
    do in ActivityListener.ShouldListenTo(ActivitySource source) or EventListener.EnableEvents(EventSource))
    might have some small performance benefits, but probably not sufficient to justify API complexity on its
    own. Storing a subscriber list per metric costs at least one pointer per metric (to store a pointer to the
    list). A nieve implementation would likely cost ~5 pointers per metric because each metric would reference a
    unique list object. However knowing that there are unlikely to be more than a few listeners ever registered
    means there are also very few possible lists. A small intern table of <10 subscriber lists would probably be
    all the unique lists most processes ever needed. Likewise there are increased CPU costs to enumerate a list
    of potentially 1000s of metrics vs. 10s of factories, but listener subscribe/unsubscribe operations are
    expected to be quite rare. The startup costs of even a large list of metrics is likely to be <1ms as long
    as per-metric filter function is reasonably efficient. Not doing per-metric filtering might mean substantial
    irrelevant data collection occurs during steady-state operation where the performance goals are likely much
    higher.
  - This leaves two reasons for factories that I think might be valuable: doing it to aggregate common metadata/
    configuration for a set of related metrics in a component and doing it just to match a past API precedent.
    We probably need to do more investigation of existing coding patterns and libraries to figure out how common
    this is. Also if the goal is only common metadata, for example to capture library name and version we might
    want to use a shared config object rather than a factory. This might look like:
````C#
TelemetrySource source = new TelemetrySource("HatterCorp.HatViewer", "v2.0.1");
source.AddTag("BetaBuild", "true");

Gauge g = new Gauge(source, "temperature");
Counter c = new Counter(source, "hats-sold");
````

## Should we standardize that all dimension values are strings or do we need to allow a broader set of types?

Early on the System.Diagnostics.Activity type used strings for its Tags property but OpenTelemetry wanted
to support non-string types such as numbers, lists, maps, etc. We needed to create alternate APIs to
allow users to pass in these other data types which made the API messier than it would have been if we
had designed for this requirement up-front. Metric dimension values appear similar to tags so it is
natural to wonder would we be making the same mistake if we defined them to be strings?

Proposed answer: Yes, we should standardize dimension values as strings

Email discussion:

From: Sergii Lavrinenko <selavrin@microsoft.com>

Makes sense to me. I also propose to decide on maximum number of dimensions, size of dimension names and values. Very high volumes increase load to client and back end and makes it hard to consume as well (cluttered UI, queries are slow etc.).

As a n example, from back-end side MDM currently supports 74 dimensions 1k characters each. Most users use 6-10 dimensions and max 100 characters. However, occasionally we see users trying to report 100+ dimensions and call stack as a string or full URL in the dimension value. I can collect more precise statistics about dimensions usage if interested.

Sergii


From: Noah Falk <noahfalk@microsoft.com>

I think it is reasonable for us to limit dimension values to be strings only. The primary intent of dimensions is to allow joining for aggregation which means we want the equality operation to be both intuitive to developers and fast to compute. If the API lets a user accidentally specify response_code = (int)200 and elsewhere response_code = (string)"200" now the SDK has to decide are those dimensions joinable or distinct? If they are joinable then we have to determine what is the exact set of type conversions the equality operator needs to consider and we make it run slower. If they aren't joinable then users will probably be confused.

Other metric libraries also appear to use strings so we'd be following precedent as well: prometheus-net/Collector.cs at 4dc0b83b280395bc114c5530fa9843a244804f9b · prometheus-net/prometheus-net (github.com)

HTH,

-Noah


From: Tarek Mahmoud Sayed <Tarek.Mahmoud.Sayed@microsoft.com>

Thanks Reiley,

 

Is there any requirement for the dimensions (for example, does it need to be serialized)?

 

my main concern is that if let’s say .NET 6 has metrics API that only allow string as dimension value, and in .NET 7 we need to support int/enum
 

I would say we can have the API in 6.0 accept Object and restrict it to strings only. In the future we can start allowing any other types as needed. I think this would be better approach than introducing more APIs to support other types.

 

Thanks,

Tarek

 

From: Reiley Yang <reyang@microsoft.com>

(merging threads)

 

@Tarek I was thinking about the following scenario:

Library reports HTTP request latency, with HTTP status code as one of the dimensions. Application owner would want to know “what’s my P95 (95%) latency for all the succeeded HTTP requests” (e.g. HTTP 2xx status).
A storage service reports READ operation metrics {storage account, blob size, latency}. Operator would want to know “what’s the maximum latency for blob < 1K, and what’s the maximum latency for blob < 100K.
 

@Victor my main concern is that if let’s say .NET 6 has metrics API that only allow string as dimension value, and in .NET 7 we need to support int/enum – would that be simply adding some overload functions or we have to invent a different API. I will poke the owner of Prometheus and Micrometer to see what’s their thinking. @Sergii do you see a trend?

 

Regards,

Reiley


From: Tarek Mahmoud Sayed Tarek.Mahmoud.Sayed@microsoft.com

What would be the scenario/reason to support non-string types? Does using numbers solve any usage problem? Or it is just to allow flexibility?


From: Victor Lu <victlu@microsoft.com>

What’s your opinion on string vs. other types for dimension value?
 

My simple answer, at this time, is to keep it a string, BUT…

 

If your question is in scope of the DIMENSION VALUE. Then there are a bunch of considerations:

The cardinality of the dimension value needs to be finite and minimized.  (At least from the backend systems I’ve experienced before).  Using an unbounded value can cause exponential growth in # of aggregates / time series the system needs to maintain.  This is true for strings as well.
One example is HTTP Status Code of 200, 404, 500, etc.  These are not “int” values, but rather is a 1:1 mapping with a finite set of enumerable status code of OK, NOTFOUND, SERVERERROR.  Thus, it’s a finite set of discrete values, easily represented as a string.
Also, we are not expected to do mathematical operations on the dimension value.  i.e. We don’t expect to add 200 with 404 for a render of 604.
Depending on how the backend system is designed, it may be more prudent to pivot the int values to be part of the metric name instead.
One example is “Bytes received” as a Dimension.  This is truly infinite and really does not belong as a dimension.  It should be reported as an independent metric itself.
In some use cases, customer may find it convenient to “TAG” a reported metric with these dimensional values.  But I think that is an error and an educational opportunity.
If the concern is memory usage and/or cpu time for processing.  It may make sense to store as a generic (i.e. DimValue<T>).  But the “value” should still be treated as discrete rather than numeric.  And some form of ToString() is still required for serialization.
Remember also, current OTLP representation of exported data still has numerics serialized into string format on the wire.
 

If the desire is to carry additional “State” info…  Or add extensibility…

We should do so outside of the Dimension/Labels.  Maybe explicitly introduce a “userdata” field per metric.
We should carry metadata as an object (or a subclass of a base class).  A Dictionary<string, BaseMetadata> semantic works well in this case.  Vendors can derived from BaseMetadata while maintaining strong typing.
These states should be designed to stay in-process and for purpose of “extending” the pipeline.  If it leave our process-space, then, more concerns on serialization and deserialization comes into play!
It is possible (and maybe even recommended?) for a vendor-specific implementation to render additional metrics or enrich labels (dimension name/value) based on known “states”.
 

If we just allow Dimension Values to be any “object”.  It will likely encourage wrongful usage. As well as render the field opaque to stages in the pipeline.  This does not promote interoperability.

 

Hopefully this makes sense as my words are likely not expressive enough.  ☹

 

VL


From: Reiley Yang <reyang@microsoft.com>

Happy President Day!

 

I don’t see anything super urgent. It would be great if @David @Noah @Tarek @Victor could comment on the highlighted part.
...
One thing I need feedback - what’s your opinion on string vs. other types for dimension value? https://github.com/open-telemetry/opentelemetry-specification/issues/1113#issuecomment-777881385
Do you think .NET should only support string as metric dimension, or we allow more types (limited set of types, or any type)?
My answer would be yes – I think we should support other types (e.g. int), so please call out if you think I’m going to a wrong direction.
 

Regards，

Reiley


## Can we use double as our sole interchange type when communicating numeric measurements between instrumented code and SDK?

When instrumented code initially calculates a measurement to be recorded it could have any integral type
such as int, float, or long. This measurement then needs to be communicated through the API layer to the
SDK as a method parameter, then stored by the SDK. Each step of this might need to convert the data type
or provide different options for the data type. For simplicity sake it would be easiest to use just one
interchange type in the API. Alternatives to using double only would be using a discriminated union type
(MetricValue struct), using a generic type parameter, or defining multiple overloads that take different
parameter types.

Proposed answer: Yes, we can standardize on double as our interchange type for numeric measurements.

Investigation:

What do the other common metric libraries do here?

Micrometer uses double in the API. Example: 
 - https://github.com/micrometer-metrics/micrometer/blob/master/micrometer-core/src/main/java/io/micrometer/core/instrument/composite/CompositeDistributionSummary.java#L37

Prometheus uses double in the API. Example: 
 - https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/ICounter.cs#L5
 - https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/Gauge.cs#L31

OpenMetrics uses int or floating point on the wire format. Collection API MAY only support float64. Reference:
 - https://github.com/OpenObservability/OpenMetrics/blob/master/specification/OpenMetrics.md#values

If we assume that all values start as one of int/long/float/double, then they get cast to
a double to cross the API->SDK interface, then the SDK casts them back to int/long/float/double what is the perf
overhead of doing that and what rounding error occurs on the integer types?

int and float can be perfectly represented by double, long in the range [-2^53, 2^53] can be perfectly represented,
outside that range a long of magnitude <= 2^X will be rounded to the nearest increment of 2^(X-53). Alternately large
long values casted to double have a margin of error of 0.00000000000001%. When visualized as an absolute value on a
graph or compared to some alert threshold it seems extremely unlikely that such a small error would ever be meaningful.
When used as a counter the difference between succesive measurements is used which would accentuate small errors, but it
is still hard to envision how it would matter in any common case. Even a counter that recorded an increment every second 
for 10 years would need to increment in value by more than 28M/sec to reach 2^53. This gives an error of 1 in 28M when
displaying the rate.

My attempts to benchmark trivial Meters that require int->double, long->double, int->double->int and long->double->long
conversions show that the overheads if they even exist, were neglible. Each iteration recorded 1000 measurements so it
is ~13ns per measurement regardless of the data type conversions.

|             Method |     Mean |    Error |   StdDev |
|------------------- |---------:|---------:|---------:|
|                Int | 12.98 us | 0.028 us | 0.026 us |
|        IntToDouble | 13.06 us | 0.101 us | 0.095 us |
|               Long | 13.11 us | 0.136 us | 0.106 us |
|       LongToDouble | 13.11 us | 0.081 us | 0.072 us |
|   IntToDoubleToInt | 12.97 us | 0.035 us | 0.029 us |
| LongToDoubleToLong | 13.09 us | 0.091 us | 0.085 us |


If the SDK had to implement an a listener pattern where the MeasurementRecorded function
uses a generic T type parameter for the measured value, how easy/hard is that to implement?
In my chat with Victor it sounded like it gets a little messy, but tractable if we needed to.


## LabelSet performance vs usability vs flexibility

Given pass experiences, handling and management of LabelSet will be a hot topic for discussion.  Need to find the right balance amongst the competing tradeoffs.


#### Option 1: pass using individual parameters.

    ```
    Record(..., 
        key1, value1, key2, value2, ...
        );
    ```

#### Option 2: pass using Dictionary

    - Option 2a: ReadOnlyDictionary
        ```
        Record(..., 
            new ReadOnlyDictionary<string,string>(
                new Dictionary<string,string>() {
                    { key1, value1 },
                    { key2, value2 },
                })
            );
        ```

    - Option 2b: ImmutableDictionary?
        ```
        var labels = ImmutableDictionary
            .CreateBuilder<string,string>()
            .Add(key1, value1)
            .Add(key2, value2)
            .ToImmutable();

        Record(..., labels);
        ```

#### Option 3: pass using string array.

    ```
    Record(..., 
        new string[] { key1, value1, key2, value2 }
        );
    ```

#### Option 4: Pass as Columnar-wise (versus Row-wise)

    ```
    Record(..., 
        keys: new string[] { key1, key2 }, 
        values: new string[] { value1, value2 },
        );
    ```


# SDK questions


## Recording a measurement with constant time/space

### Problem Statement
When a measurement is recorded, the calculation/accumulation/aggregation is
process synchronouosly per call. Thus, the timing for each recording can vary
based on how the SDK is configured.  It may be desireable to have a constant
time/space per recording.

### Proposal
Decouple the recording of measurments from the calculation/accumulation/aggregation
of these measurements. One approach is to put a Queue between the concerns.
Recorded measurements are enqueued at constant time/space. A concurrent thread/task
can dequeue measurements and do appropriate calculations and processing.


# Graveyard

I am moving discussion here when they don't appear to be currently relevant. If the become relevant again we can
always move them back.

## Can batching reuse mostly the same APIs we would use for non-batched recording?

This question is superceded by the more fundamental question above, "Should we have batching?"

OT defines a RecordBatch() API that relies on special Measurement() APIs on each instrument type. What if we just
define an API that starts and stops a batch?

````C#
Metrics.BeginBatch();
metric1.Add(123);
metric2.Set(19);
...
Metrics.EndBatch();

// another more idiomatic C# variation of this could be:
using(new MetricBatch())
{
    metric1.Add(123);
    metric2.Set(19);
    ...
}
````

From a usability perspective this doesn't require API consumers to record metrics any differently than they
would have done outside a batch, aside from the Begin/End call itself. It also means that batches can wrap arbitrary
amounts of code to be more easily adopted when needed. From an implementation ease perspective I don't know if all the
languages OpenTelemetry cares about support a variadic syntax. C# has the params keyword which appears variadic from the
callers perspective but it allocates an array on the heap. From a performance perspective this pattern probably uses
more CPU to make more calls across the API->SDK boundary but doesn't need to allocate temporary storage. There are
probably both scenarios that benefit and scenarios that regress.

## Metric API to follow EventSource / EventListener design pattern

This one wasn't framed as a question. If anyone wants to reframe it so it is more apparent what are the
alternatives we are selecting between I'm happy to restore it and discuss.

- This design pattern has been well established with .NET runtime.  (i.e. EventCounter, ETW, TraceProvider, etc...)
- This is a potential argument for using a factory pattern. Should we fold this into that question?
