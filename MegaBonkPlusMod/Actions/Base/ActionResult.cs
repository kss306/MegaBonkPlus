namespace MegaBonkPlusMod.Actions.Base;

public class ActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public static ActionResult Ok(string message)
    {
        return new ActionResult { Success = true, Message = message };
    }

    public static ActionResult Fail(string message)
    {
        return new ActionResult { Success = false, Message = message };
    }
}