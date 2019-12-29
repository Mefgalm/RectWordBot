open System
open Funogram.Api
open Funogram.Telegram.Api
open Words.Word
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open Words.Meme

type OptionBuilder() =
    member this.Bind(x, f) = Option.bind f x
    member this.Return(x) = Some x
    member this.Zero () = None
    
let option = OptionBuilder()    

let apiSend config f =
    f()
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
        
        apiSend
            context.Config
            (fun () -> sendMessageBase (ChatId.Int message.Chat.Id) text (Some ParseMode.Markdown) None None None None)
    }
    |> ignore
    
let onHelp context =
    option {
        let! message = context.Update.Message
        let result = "/start <border word> <inner word>
example:
/start HAPPYNEWYEAR 2020

/meme"
        
        apiSend
            context.Config
            (fun () -> sendMessage message.Chat.Id result)
    }
    |> ignore
    
let onMeme (context: UpdateContext) =
    option {
        let! message = context.Update.Message
        let newMeme = getMeme()
        
        apiSend
            context.Config
            (fun () -> sendPhoto message.Chat.Id (FileToSend.Url <| Uri newMeme.url) newMeme.title)
    }
    |> ignore

let onUpdate (context: UpdateContext) =
  processCommands context [
    cmd "/help" onHelp
    cmd "/meme" onMeme
    cmdScan "/start %s %s" (onStart context)
  ]
  |> ignore


[<EntryPoint>]
let main argv =
    startBot {
        defaultConfig with
          Token = Environment.GetEnvironmentVariable("BotToken")
    } onUpdate None
    |> Async.RunSynchronously
    |> ignore
    
    0 
