﻿#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetwork.Telegram.Utils.CallbackUtils;
using PoliNetworkBot_CSharp.Code.Objects;
using SampleNuGet.Objects;
using SampleNuGet.Utils.CallbackUtils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils.CallbackUtils;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class CallbackAssocVetoData : CallbackGenericData
{
    public readonly string? Message;
    public readonly MessageEventArgs? MessageEventArgs;
    public readonly string? MessageWithMetadata;
    public bool Modified;

    public CallbackAssocVetoData(List<CallbackOption> options, Action<CallbackGenericData> runAfterSelection,
        string? message, MessageEventArgs? messageEventArgs, string? messageWithMetadata) : base(options,
        runAfterSelection)
    {
        MessageWithMetadata = messageWithMetadata;
        MessageEventArgs = messageEventArgs;
        Message = message;
        Options = options;
        RunAfterSelection = runAfterSelection;
    }

    public void OnCallback()
    {
        Modified = true;
    }
}