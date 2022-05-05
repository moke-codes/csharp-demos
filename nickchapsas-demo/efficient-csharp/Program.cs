
using BenchmarkDotNet.Running;

using EfficientCsharp;

BenchmarkRunner.Run<GuiderBenchmarks>();
//
// var id = Guid.NewGuid();
// Console.WriteLine(id);
//
// var base64Id = Convert.ToBase64String(id.ToByteArray()); 
// Console.WriteLine(base64Id);
//
// var urlFriendlyBase64Id = Guider.ToStringFromGuid(id);
// var optimizedUrlFriendlyBase64Id = Guider.ToStringFromGuidOptimized(id);
// Console.WriteLine(urlFriendlyBase64Id);
// Console.WriteLine(optimizedUrlFriendlyBase64Id);
//
// var idAgain = Guider.ToGuidFromString(urlFriendlyBase64Id);
// var optimizedIdAgain = Guider.ToGuidFromStringOptimized(urlFriendlyBase64Id);
// Console.WriteLine(idAgain);
// Console.WriteLine(optimizedIdAgain);