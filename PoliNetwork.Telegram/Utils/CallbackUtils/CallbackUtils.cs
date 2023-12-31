﻿#region

using System.Numerics;
using Newtonsoft.Json;
using PoliNetwork.Telegram.Utils.CallbackUtils;
using SampleNuGet.Objects;
using SampleNuGet.Objects.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace SampleNuGet.Utils.CallbackUtils;

public static class CallbackUtils
{
    private const string Separator = "-";
    public static CallBackDataFull? CallBackDataFull = new();

    public static async Task<MessageSentResult?> SendMessageWithCallbackQueryAsync(
        CallbackGenericData callbackGenericData,
        long chatToSendTo,
        Language text, TelegramBotAbstract? telegramBotAbstract, ChatType chatType, string? lang, string? username,
        int? messageThreadId,
        bool splitMessage, long? replyToMessageId = null)
    {
        callbackGenericData.Bot = telegramBotAbstract;
        callbackGenericData.InsertedTime = DateTime.Now;

        var newLast = GetLast();
        var key = GetKeyFromNumber(newLast);
        callbackGenericData.Id = key;
        CallBackDataFull?.Add(key, callbackGenericData);

        var replyMarkupObject = GetReplyMarkupObject(callbackGenericData, key);
        if (telegramBotAbstract == null) return null;
        var messageSent = await telegramBotAbstract.SendTextMessageAsync(chatToSendTo, text, chatType, lang,
            ParseMode.Html, replyMarkupObject, username, splitMessage: splitMessage,
            replyToMessageId: replyToMessageId, messageThreadId: messageThreadId);
        callbackGenericData.MessageSent = messageSent;

        return messageSent;
    }

    internal static void DoCheckCallbackDataExpired()
    {
        while (true)
        {
            try
            {
                CallBackDataFull?.CheckCallbackDataExpired();
            }
            catch
            {
                // ignored
            }

            Thread.Sleep(1000 * 60 * 60 * 24); //every day
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static ReplyMarkupObject GetReplyMarkupObject(CallbackGenericData callbackGenericData, string? key)
    {
        var x2 = callbackGenericData.Options.Select((option, i1) => new List<InlineKeyboardButton>
            { new(option.displayed) { CallbackData = key + Separator + i1 } }).ToList();

        var inlineKeyboardMarkup = new InlineKeyboardMarkup(x2);
        return new ReplyMarkupObject(inlineKeyboardMarkup);
    }

    private static string? GetKeyFromNumber(BigInteger? newLast)
    {
        var r = newLast?.ToString("X");
        return r;
    }

    private static BigInteger? GetLast()
    {
        return CallBackDataFull?.GetLast();
    }

#pragma warning disable CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.

    public static void CallbackMethodStart(object? sender, CallbackQueryEventArgs e)
#pragma warning restore CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.
    {
        try
        {
            var t = new Thread(() => _ = CallbackMethodHandle(sender, e));
            t.Start();
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
        }
    }

#pragma warning disable CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.

    private static async Task<bool> CallbackMethodHandle(object? sender, CallbackQueryEventArgs callbackQueryEventArgs)
#pragma warning restore CS8632 // L'annotazione per i tipi riferimento nullable deve essere usata solo nel codice in un contesto di annotations '#nullable'.
    {
        TelegramBotAbstract? telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClient = new TelegramBotAbstract(tmp);

            if (telegramBotClient == null)
                return false;

            await CallbackMethodRun(telegramBotClient, callbackQueryEventArgs);
        }
        catch (Exception? exc)
        {
            await NotifyUtil.NotifyOwnersWithLog(exc, telegramBotClient, null,
                EventArgsContainer.Get(callbackQueryEventArgs));
        }

        return false;
    }

    private static async Task CallbackMethodRun(TelegramBotAbstract? telegramBotClientBot,
        CallbackQueryEventArgs callbackQueryEventArgs)
    {
        try
        {
            if (callbackQueryEventArgs.CallbackQuery != null)
            {
                var data = callbackQueryEventArgs.CallbackQuery.Data;
                if (string.IsNullOrEmpty(data) == false)
                {
                    string?[] datas = data.Split(Separator);
                    var key = datas[0];
                    var answer = Convert.ToInt32(datas[1]);
                    CallBackDataFull?.UpdateAndRun(callbackQueryEventArgs, answer, key);
                }
            }
        }
        catch (Exception? exception)
        {
            await NotifyUtil.NotifyOwnersWithLog(exception, telegramBotClientBot, null,
                EventArgsContainer.Get(callbackQueryEventArgs));
        }
    }

    internal static void InitializeCallbackDatas()
    {
        try
        {
            try
            {
                CallBackDataFull = JsonConvert.DeserializeObject<CallBackDataFull>(
                    File.ReadAllText(Paths.Data.CallbackData));
                Logger.Logger.WriteLine("Callbackdata file is empty", default);
            }
            catch (Exception ex)
            {
                File.Create(Paths.Data.CallbackData);
                Logger.Logger.WriteLine("Initialized CallbackData file");
                CallBackDataFull = new CallBackDataFull();
            }
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
        }
    }
}