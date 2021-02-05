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

## C# Interface versus abstract/base classes and inheritance

### 02-04-2021

- .NET team has recommendations and guidance
  - Use concrete classes for simplicity
  - @Noah, please provide more rationale here...


## API with a starting Provider class with factory to create Meters versus creating Meters directly

### 02-04-2021

- API to follow design patterns established with Trace Provider API


## Metric API to follow EventSource / EventListener design pattern

### 02-04-2021

- This design pattern has been well established with .NET runtime.  (i.e. EventCounter, ETW, TraceProvider, etc...)


## Passing native numbers (int/doubles) versus boxing into a MetricValue class

### 02-04-2021

- API allows for native numbers as input parameters, but is BOXed into MetricValue before passing to SDK.
  - Q: Boxing vs struct.  Performance?  Usability?  Maintainability?
  - Options: Use Generics?

- If using MetricValue, need to explore "union" struct/class (i.e. FieldOffsetAttribute)


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
