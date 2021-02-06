# Summary

An experimental project to implement an OpenTelemetry Metrics API and SDK.

Our goals are:

- To discover/design a simpler approach to Metrics API
- To explore different SDK implementation based on Metrics API design
- To experiment with alternative designs/models/ideas for evaluation
- To complement existing/current .NET runtime design
- To maintain zero/minimal differences between OpenTelemetry specification and .NET runtime
- To use this project as an informed starting base for discussion with wider OpenTelemetry community

# Topics

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

## Can batching reuse mostly the same APIs we would use for non-batched recording?

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

## C# Interface versus abstract/base classes and inheritance

- The .NET team has [framework design guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) 
that provide suggestions for API design. All else being equal we would recommend to expose concrete types that
a developer can create via the 'new' operator rather than interfaces, abstract types, or factory patterns. Largely this
is to minimize the number of concepts or abstractions that a developer needs to understand in order to use the API.

For more info see:
https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/type
https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/interface


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


## Metric API to follow EventSource / EventListener design pattern

- This design pattern has been well established with .NET runtime.  (i.e. EventCounter, ETW, TraceProvider, etc...)
- This is a potential argument for using a factory pattern. Should we fold this into that question?


## Passing native numbers (int/doubles) versus boxing into a MetricValue class

### 02-04-2021

- API allows for native numbers as input parameters, but is BOXed into MetricValue before passing to SDK.
  - Q: Boxing vs struct.  Performance?  Usability?  Maintainability?
  - Options: Use Generics?

- If using MetricValue, need to explore "union" struct/class (i.e. FieldOffsetAttribute)

TODO - Research project: If we assume that all values start as one of int/long/float/double, then they get cast to
a double to cross the API->SDK interface, then the SDK casts them back to int/long/float/double what is the perf
overhead of doing that and what rounding error occurs on the integer types?

TODO - Research project: If the SDK had to implement an a listener pattern where the MeasurementRecorded function
uses a generic T type parameter for the measured value, how easy/hard is that to implement?

TODO - Research project: What do the other common metric libraries do here?


## LabelSet performance vs usability vs flexibility

Given pass experiences, handling and management of LabelSet will be a hot topic for discussion.  Need to find the right balance amongst the competing tradeoffs.

### 02-04-2021

- Option 1: pass using individual parameters.

    ```
    Record(..., 
        key1, value1, key2, value2, ...
        );
    ```

- Option 2: pass using Dictionary

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

- Option 3: pass using string array.

    ```
    Record(..., 
        new string[] { key1, value1, key2, value2 }
        );
    ```

- Option 4: Pass as Columnar-wise (versus Row-wise)

    ```
    Record(..., 
        keys: new string[] { key1, key2 }, 
        values: new string[] { value1, value2 },
        );
    ```

## Recording a measurement with constant time/space

### Problem Statement
When a measurement is recorded, the calculation/accumulation/aggregation is process synchronouosly per call.
Thus, the timing for each recording can vary based on how the SDK is configured.  It may be desireable to have a constant
time/space per recording.

### Proposal
Decouple the recording of measurments from the calculation/accumulation/aggregation of these measurements.
One approach is to put a Queue between the concerns.  Recorded measurements are enqueued at constant time/space.
A concurrent thread/task can dequeue measurements and do appropriate calculations and processing.

Does everyone agree that this is an SDK implementation concern rather than an API concern? I propose we segregate our design
questions into three categories:
1. .NET API
2. .NET implementation
3. SDK implementation

I would then prioritize resolving the issues in that order, with the caveat that we may want to make an API choice
based on the result of resolving particular implementation questions.


