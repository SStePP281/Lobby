using System;

[Serializable]
public class BaseMessage
{
    public string Type;
}

[Serializable]
public class InviteRequest : BaseMessage
{
    public InviteRequest() : this(string.Empty) { }

    public InviteRequest(string senderId) : this(nameof(InviteRequest), senderId) { }

    private InviteRequest(string type, string senderId)
    {
        Type = type;
        SenderId = senderId;
    }

    public string SenderId;
}

[Serializable]
public class InviteResponse : BaseMessage
{
    public InviteResponse() : this(string.Empty) { }

    public InviteResponse(string senderId) : this(nameof(InviteResponse), senderId) {}

    private InviteResponse(string type, string senderId)
    {
        Type = type;
        SenderId = senderId;
    }

    public string SenderId;
}

[Serializable]
public class InviteCancellation : BaseMessage
{
    public InviteCancellation() : this(string.Empty) { }

    public InviteCancellation(string senderId) : this(nameof(InviteCancellation), senderId) { }

    private InviteCancellation(string type, string senderId)
    {
        Type = type;
        SenderId = senderId;
    }

    public string SenderId;
}

[Serializable]
public class LobbyIdMessage : BaseMessage
{
    public LobbyIdMessage() : this(string.Empty) { }

    public LobbyIdMessage(string lobbyId) : this(nameof(LobbyIdMessage), lobbyId) { }

    private LobbyIdMessage(string type, string lobbyId)
    {
        Type = type;
        LobbyId = lobbyId;
    }

    public string LobbyId;
}