namespace NetcoreFSL;

public class Fsl
{
  public Fsl()
  {
    Name = "anonymous";
  }

  public string Name { get; set; }
  public string TestGreeting => "Hello, " + Name + "!";
}
