namespace TrickModule.Core
{
    public enum DispatchContainerType
    {
        WaitForFixedUpdate,
        WaitForEndOfFrame,
        WaitForNewFrame,

        Reserved = 1000,
    }
}