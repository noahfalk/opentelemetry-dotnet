# Prometheus

```
import io.prometheus.client.Counter;
class YourClass {
  static final Counter requests = Counter.build()
     .name("requests_total").help("Total requests.")
     .labelNames("path")
     .register();

  void processRequest() {
    requests.inc();
    // Your code here.
  }
}
```

Types: Counter, Guage, Summary, Histogram

# Micrometer

```
MeterRegistry registry = new JmxMeterRegistry(new JmxConfig() {
    @Override
    public String get(String s) {
        return null;
    }
}, Clock.SYSTEM);

Counter counter = Counter
        .builder("my.counter")
        .description("counts something important")
        .tag("environment", "test")
        .tag("region", "us-east")
        .register(registry);

counter.increment(); // increment by one
counter.increment(2.5);
```

Types: Counter, Guage, Timer, Distrubution Summary, Long Task Timer, Histogram and Percentiles

# DropWizard

```
  package sample;
  import com.codahale.metrics.*;
  import java.util.concurrent.TimeUnit;

  public class GetStarted {
    static final MetricRegistry metrics = new MetricRegistry();
    public static void main(String args[]) {
      startReport();
      Meter requests = metrics.meter("requests");
      requests.mark();
      wait5Seconds();
    }

  static void startReport() {
      ConsoleReporter reporter = ConsoleReporter.forRegistry(metrics)
          .convertRatesTo(TimeUnit.SECONDS)
          .convertDurationsTo(TimeUnit.MILLISECONDS)
          .build();
      reporter.start(1, TimeUnit.SECONDS);
  }

  static void wait5Seconds() {
      try {
          Thread.sleep(5*1000);
      }
      catch(InterruptedException e) {}
  }
```

Types: Counter, Guage, Timer, Histogram, Health Check

# DogStatsD

```
import com.timgroup.statsd.NonBlockingStatsDClientBuilder;
import com.timgroup.statsd.StatsDClient;
import java.util.Random;

public class DogStatsdClient {

    public static void main(String[] args) throws Exception {

        StatsDClient Statsd = new NonBlockingStatsDClientBuilder()
            .prefix("statsd")
            .hostname("localhost")
            .port(8125)
            .build();

        for (int i = 0; i < 10; i++) {
            Statsd.incrementCounter("example_metric.increment", new String[]{"environment:dev"});
            Statsd.decrementCounter("example_metric.decrement", new String[]{"environment:dev"});
            Statsd.count("example_metric.count", 2, new String[]{"environment:dev"});

            Statsd.recordGaugeValue("example_metric.gauge", i, new String[]{"environment:dev"});

            Thread.sleep(100000);
        }
    }
}
```

Types: Count, Rate, Gauge, Histogram, Distribution

# New Relic

```
public class CountExample {

  private static final ThreadLocalRandom random = ThreadLocalRandom.current();

  private static final List<String> items = Arrays.asList("apples", "oranges", "beer", "wine");

  public static void main(String[] args) throws Exception {
    String insightsInsertKey = args[0];

    MetricBatchSenderFactory factory =
        MetricBatchSenderFactory.fromHttpImplementation(OkHttpPoster::new);
    MetricBatchSender sender =
        MetricBatchSender.create(factory.configureWith(insightsInsertKey).build());
    MetricBuffer metricBuffer = new MetricBuffer(getCommonAttributes());

    for (int i = 0; i < 10; i++) {
      String item = items.get(random.nextInt(items.size()));
      long startTimeInMillis = System.currentTimeMillis();

      TimeUnit.MILLISECONDS.sleep(5);

      Count purchaseCount = getPurchaseCount(startTimeInMillis, item);
      System.out.println("Recording purchase for: " + item);

      metricBuffer.addMetric(purchaseCount);
    }

    sender.sendBatch(metricBuffer.createBatch());
  }

  /** These attributes are shared across all metrics submitted in the batch. */
  private static Attributes getCommonAttributes() {
    return new Attributes().put("exampleName", "CountExample");
  }

  private static Count getPurchaseCount(long startTimeInMillis, String item) {
    return new Count(
        "purchases",
        random.nextDouble(50, 500),
        startTimeInMillis,
        System.currentTimeMillis(),
        getPurchaseAttributes(item));
  }

  private static Attributes getPurchaseAttributes(String item) {
    Attributes attributes = new Attributes();
    attributes.put("item", item);
    attributes.put("location", "downtown");
    return attributes;
  }
}
```

Types: Count, Gauge, Summary

# AWS CloudWatch

```
aws cloudwatch put-metric-data --metric-name Buffers --namespace MyNameSpace --unit Bytes --value 231434333 --dimensions InstanceId=1-23456789,InstanceType=m1.small

aws cloudwatch put-metric-data --metric-name PageViewCount --namespace MyService --value 2 --timestamp 2016-10-20T12:00:00.000Z
aws cloudwatch put-metric-data --metric-name PageViewCount --namespace MyService --value 4 --timestamp 2016-10-20T12:00:01.000Z
aws cloudwatch put-metric-data --metric-name PageViewCount --namespace MyService --value 5 --timestamp 2016-10-20T12:00:02.000Z

aws cloudwatch put-metric-data --metric-name PageViewCount --namespace MyService --statistic-values Sum=11,Minimum=2,Maximum=5,SampleCount=3 --timestamp 2016-10-14T12:00:00.000Z
```

Types: Gauge

# Azure Monitor: Application Insigts

```
private TelemetryClient telemetry = new TelemetryClient();

var sample = new MetricTelemetry();
sample.Name = "metric name";
sample.Value = 42.3;
telemetry.TrackMetric(sample);

// Set up some properties and metrics:
var properties = new Dictionary <string, string>
    {{"game", currentGame.Name}, {"difficulty", currentGame.Difficulty}};
var metrics = new Dictionary <string, double>
    {{"Score", currentGame.Score}, {"Opponents", currentGame.OpponentCount}};

// Send the event:
telemetry.TrackEvent("WinGame", properties, metrics);
```

Types: Gauge

# MDM IFx

```
MdmMetricController.AddDefaultDimension("FixedDim", "FixedDimValue");
MdmMetricController.StartMetricPublication();

var factory = new MdmMetricFactory();
var metric4d = factory.CreateUInt64Metric(MdmMetricFlags.MetricDefault, "TestAccount", "TestNamespace",
                "TestMetric4D", "DimName1", "DimName2", "DimName3", "DimName4");

var dimValues2 = DimensionValues.Create("DimVal1", "DimVal2", "DimVal3", "DimVal4");
metric4d.Set(100, dimValues2);
```

Types: Metrics, CumulativeMetric, CumulativeDistributionMetric, PassthroughMetic
