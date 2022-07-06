namespace STUN.Enums
{
    /// <summary>
    /// Although there are 4 message classes, there are only two types of transactions in STUN:
    /// request/response transactions (which consist of a request message and a response message)
    /// and indication transactions(which consist of a single indication message)
    /// 
    /// besides
    /// response classes are split into error and success responses to aid in quickly processing the STUN message.
    /// 
    ///     0                 1
    ///     2  3  4 5 6 7 8 9 0 1 2 3 4 5
    ///    +--+--+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    |M |M |M|M|M|C|M|M|M|C|M|M|M|M|
    ///    |11|10|9|8|7|1|6|5|4|0|3|2|1|0|
    ///    +--+--+-+-+-+-+-+-+-+-+-+-+-+-+
    ///    
    /// C1 and C0 represent a 2-bit encoding of the class.  4种排列组合
    /// A class of 0b00 is a request
    /// a class of 0b01 is an indication
    /// a class of 0b10 is a success response, and a class of 0b11 is an error response
    /// </summary>
    public enum StunMessageClass
    {
        Request = 0b00000_0_000_0_0000,
        Indication = 0b00000_0_000_1_0000,
        SuccessResponse = 0b00000_1_000_0_0000,
        ErrorResponse = 0b00000_1_000_1_0000,
    }
}
