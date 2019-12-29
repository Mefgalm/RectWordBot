module Words.Meme
open System.Text.Json.Serialization
open FSharp.Data
open Newtonsoft.Json

let apiUrl = "https://meme-api.herokuapp.com/gimme"

type MemeResponse =
    { postLink: string
      subreddit: string
      title: string
      url: string }
    
let getMeme() =
    let response = Http.RequestString apiUrl
    JsonConvert.DeserializeObject<MemeResponse>(response)
