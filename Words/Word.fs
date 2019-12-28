module Words.Word

open System

let reverseStr (str: string) =
    str |> Seq.rev |> System.String.Concat
    
let buildRect (word: string) =
    let size = word.Length - 1
    let revertedWord = word |> reverseStr

    seq {
        for y in 0..size ->
            seq {
                for x in 0..size ->
                        match x, y with
                        | _, 0  ->  word.[x]
                        | _, cy when cy = size -> revertedWord.[x]
                        | 0, _ -> word.[y]
                        | cx, _ when cx = size -> revertedWord.[y]
                        | _ -> ' '
            }
    }
    |> Seq.map Seq.toArray
    |> Seq.toArray    
    
let (|@|) arr1 arr2 = Array.append arr1 arr2
    
let makeChunkInMiddle size (chunk: char[]) =
    let leftEmptyChunkSize = (size - chunk.Length) / 2
    let rightEmptyChunkSize = size - chunk.Length - leftEmptyChunkSize
    Array.create leftEmptyChunkSize ' ' |@| chunk |@| Array.create rightEmptyChunkSize ' '
    
let splitWord size (word: string) =
    let chunkPlacer = makeChunkInMiddle size
    
    let chunks =
        word.Split([| " " |], StringSplitOptions.RemoveEmptyEntries)
        |> Seq.map (Seq.chunkBySize size)
        |> Seq.collect id
        |> Seq.map chunkPlacer
        |> Seq.toArray
    chunks |> Array.take (min size chunks.Length)

let wordToRect size (wordChunks: char[][]) =
    let firstBlockSize = (size - wordChunks.Length) / 2
    let lastBlockSize = size - wordChunks.Length - firstBlockSize
    
    let zeroArray = Array.create size ' '
    
    Array.create firstBlockSize zeroArray |@| wordChunks |@| Array.create lastBlockSize zeroArray
    
let mergeRects (backGround: char[][]) (foreGround: char[][]) =
    let size = backGround.Length
    
    let shiftSize = (backGround.Length - foreGround.Length) / 2
    
    for y in shiftSize..(size - shiftSize - 1) do
            for x in shiftSize..(size - shiftSize - 1) do
                backGround.[y].[x] <- foreGround.[y - shiftSize].[x - shiftSize]
    
    backGround
    |> Seq.map String.Concat
    |> Seq.reduce (sprintf "%s\n%s")
    
let wrapWord (rectWord: string) (word: string) =
    if word.Length < 1 || rectWord.Length < 5 then
        failwith "Rect Word length must be greater or equals 5 and Inner Word length must be greater or equals 1"
    
    let innerBlockSize = rectWord.Length - 4
    
    let rectAreaWithWord = rectWord |> buildRect
    
    let splitedWordInRect = word |> splitWord innerBlockSize |> wordToRect innerBlockSize
    
    mergeRects rectAreaWithWord splitedWordInRect
