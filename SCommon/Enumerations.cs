namespace SCommon
{
    /// <summary>
    /// The SecurityFlags enum used for <see cref="Security.Security"/> class
    /// </summary>
    public enum SecurityFlags
    {
        None = 0,
        Handshake = 1,
        Blowfish = 2,
        SecurityBytes = 4,
    }
}
