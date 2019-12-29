open System
open Funogram
open Funogram.Api
open Funogram.Telegram
open Funogram.Tools
open Funogram.Telegram.Api
open Words.Word
open Funogram.Telegram.Bot
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open Words
open Words.Meme

type OptionBuilder() =
    member this.Bind(x, f) = Option.bind f x
    member this.Return(x) = Some x
    member this.Zero () = None
    
let option = OptionBuilder()    

let sendTextMessage config chatId text =
    sendMessageBase (ChatId.Int chatId) text (Some ParseMode.Markdown) None None None None
    |> api config 
    |> Async.Ignore
    |> Async.Start

let onStart context (rectWord, word) =
    option {
        let! message = context.Update.Message
        let text =
            match wrapWord rectWord word with
            | Ok res -> sprintf "```\n%s```" res
            | Error er -> sprintf "Error: %s" er
        
        sendTextMessage context.Config message.Chat.Id text
    }
    |> ignore
    
let onHelp context =
    option {
        let! message = context.Update.Message
        let result = "/start <border word> <inner word>
example:
/start HAPPYNEWYEAR 2020

/meme"
        
        sendTextMessage context.Config message.Chat.Id result
    }
    |> ignore
    
let onHello context =
    option {
        let! message = context.Update.Message
        let! name = message.Chat.FirstName
        sprintf "Hello, %s!" name
        |> sendMessage message.Chat.Id
        |> api context.Config
        |> Async.Ignore
        |> Async.Start
    }
    |> ignore

let onMeme (context: UpdateContext) =
    option {
        let! message = context.Update.Message
        let newMeme = getMeme()
        
        sendPhoto message.Chat.Id (FileToSend.Url <| Uri newMeme.url) newMeme.title
        |> api context.Config
        |> Async.Ignore
        |> Async.Start
    }
    |> ignore

let onUpdate (context: UpdateContext) =
  processCommands context [
    cmd "/help" onHelp
    cmd "/meme" onMeme
    cmdScan "/start %s %s" (onStart context)
  ]
  |> ignore

let prodToken = "887888810:AAGHluamUkJ99X7G1dACCTL4hfcjxhQcV-I"
let testToken = "911472698:AAHCKpLvTsOyjNJD_PT-gZxsdHjVi1onJ5g"

[<EntryPoint>]
let main argv =
    startBot {
        defaultConfig with
          Token = prodToken
    } onUpdate None
    |> Async.RunSynchronously
    |> ignore
    
    0 
