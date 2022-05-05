
/* The features that make the new program simpler are top-level statements, global using directives, and implicit using directives.
 * For Console apps, the implicit usings are:
 *
 * using System;
 * using System.Collections.Generic;
 * using System.Linq;
 * using System.Net.Http;
 * using System.Net.Http.Json;
 */
using System.Net.Http.Json;

// New const interpolated strings
const string BaseUrl = "https://jsonplaceholder.typicode.com";
const string PostsUrl = $"{BaseUrl}/posts";

// New: assign lambda expression to an implicitly-typed variable
var printPost = (int id, string? body) => {
    Console.WriteLine($"Post {id}:");
    Console.WriteLine($"{body}");
    Console.WriteLine();
};

foreach (var (id, body) in await GetPosts()) 
    printPost(id, body);

static async Task<IEnumerable<Post>> GetPosts() {
    using var httpClient = new HttpClient();

    try {
        var posts = await httpClient.GetFromJsonAsync<IEnumerable<Post>>(PostsUrl);
        posts ??= Enumerable.Empty<Post>();

        return posts;
    }
    catch {
        return Enumerable.Empty<Post>();
    }
}

// Record with custom deconstruction
record Post (int UserId, int Id, string? Title, string? Body) {
    public void Deconstruct(out int id, out string? body) =>
        (id, body) = (Id, Body);
}
