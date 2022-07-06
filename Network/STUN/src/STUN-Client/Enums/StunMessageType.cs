namespace STUN.Enums
{
    /// <summary>
    ///  The message type defines 
    ///  the message class (request, successresponse, failure response, or indication)
    ///  and the message method (the primary function) 
    ///  of the STUN message
    /// </summary>
    public enum StunMessageType : ushort
    {
        BindingRequest = StunMessageClass.Request | StunMessageMethod.Binding,
        BindingResponse = StunMessageClass.SuccessResponse | StunMessageMethod.Binding,
        BindingErrorResponse = StunMessageClass.ErrorResponse | StunMessageMethod.Binding,
        SharedSecretRequest = StunMessageClass.Request | StunMessageMethod.SharedSecret,
        SharedSecretResponse = StunMessageClass.SuccessResponse | StunMessageMethod.SharedSecret,
        SharedSecretErrorResponse = StunMessageClass.ErrorResponse | StunMessageMethod.SharedSecret
    }
}
