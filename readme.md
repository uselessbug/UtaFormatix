# UtaFormatEx

**UtaFormatix** is a tool for converting vocal-synthesizer's project format among VOCALOID, UTAU and CeVIO, created by @Sdercolin . Here is a modified version of UtaFormatix - **UtaFormatEx**. There are no new features, just more cleaner different code style and implementation (in order to be used in some modern environments).

## Feature
Format:

* VSQX (Vocaloid 3-4)
* CCS (CeVIO 3+)
* UST (UTAU)

Convert Content:

* Note
* Lryic
* Tempo & Beat

Parameters are not supported currently.

## Benchmark

We took a benchmark to compare performance between [UtaFormatix](https://github.com/sdercolin/UtaFormatix/tree/220b7b97a74ea6214001a9bba8351276e137775a) and UtaFormatEx.

The test case is to convert a VSQX to CCS, and convert it back; then convert a CCS to VSQX, and convert it back. Both tools run the same job.

For the test, we have to remove all `MessageBox.Show()` in UtaFormatix. No change needed for UtaFormatEx.

Test results show that UtaFormatEx is over 50% faster than original UtaFormatix, and has less memory allocated in the mean time.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i5-6300U CPU 2.40GHz (Skylake), 1 CPU, 4 logical and 2 physical cores


```
|               Method |      Mean |    Error |   StdDev |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|--------------------- |----------:|---------:|---------:|----------:|---------:|---------:|----------:|
| UtaFormatExBenchmark |  71.45 ms | 1.408 ms | 1.383 ms |  937.5000 | 437.5000 |        - |   4.34 MB |
| UtaFormatixBenchmark | 152.35 ms | 3.190 ms | 2.984 ms | 1125.0000 | 562.5000 | 250.0000 |   6.57 MB |


---
Originally created by **[@Sdercolin](http://sdercolin.com/akatsuki/utaformatix/)**

Some code rewrited by **Ulysses**, wdwxy12345@gmail.com

---

## LICENSE

*Ask [@Sdercolin](http://sdercolin.com/) Before You Do Anything Evil* License.


