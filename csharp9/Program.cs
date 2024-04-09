using System;

Person p = new()
{
    Name = ParseName(Question("What's your name?")),
    Age = ParseAge(Question($"What's your age?"))
};

var response = p.Age switch
{
    > 0 and < 12 => $"{p.Name} is just a kid.",
    >= 12 and <= 17 => $"Oh! {p.Name} is discovering what life has to offer.",
    > 17 and <= 23 => $"Congrats {p.Name}, you're in your prime!",
    >= 24 and < 40 => "Life isn't so easy as you've imagined before, "
      + $"right Mr(s). {p.Name.Second}?",
    >= 40 => $"Hold on {p.Name}, bumps ahead!",
    _ => "What kind of age is this?!"
};

Console.WriteLine(response);

string Question(string question) {
    Console.Write(question + " ");
    return Console.ReadLine();
}

Name ParseName(string name)
{
    var parts = name.Split((char)ConsoleKey.Spacebar);
    return parts.Length switch
    {
        >= 2 => new(parts[0], parts[1]),
        1 => new(parts[0], string.Empty),
        _ => default
    };
};

int ParseAge(string value) {
    return Int32.TryParse(value, out int age) ? age : default;
}

record Name(string First, string Second)
{
    public override string ToString() => this.First;
};

record Person(Name Name = default, int Age = 0)
{
    public string FullName => $"{this.Name.First} {this.Name.Second}";
};
