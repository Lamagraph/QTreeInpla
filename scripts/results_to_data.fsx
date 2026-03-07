#!/usr/bin/env -S dotnet fsi

open System
open System.IO

let resultsPath = "experiments/results/"
let convertationPath = "experiments/data/convertation_data/"
let bfsPath = "./experiments/data/bfs_data/"

let convertationLine = 2
let bfsLine = 5

let getTime (str: string) =
    // ... interactions (by n threads), t sec)
    let words = str.Split(' ')
    let timeword = words.Length - 2
    words.[timeword]

// We can get this data from the filename
// or from the file itself
let getThreads (str: string) =
    let words = str.Split(' ')
    let threadword = 3
    int <| words.[threadword]

// Gets number of threads and time
let handleFile filePath =
    let text = File.ReadAllLines(filePath)
    let threads = getThreads text.[bfsLine]
    let bfsTime = getTime text.[bfsLine]
    let convertationTime = getTime text.[convertationLine]
    threads, bfsTime, convertationTime

let handleDirectory dir =
    let files = Directory.GetFiles(dir, "*.output")
    let data = files |> Array.map handleFile |> Array.rev

    let convertationDataFile =
        data
        |> Array.map (fun (threads, _, convertTime) -> sprintf "%d %s" threads convertTime)
        |> String.concat "\n"

    let bfsDataFile =
        data
        |> Array.map (fun (threads, bfsTime, _) -> sprintf "%d %s" threads bfsTime)
        |> String.concat "\n"

    let name = DirectoryInfo(dir).Name
    (name, bfsDataFile, convertationDataFile)


let main args =
    let dirs =
        Directory.GetDirectories(resultsPath)
        |> Array.except [ bfsPath; convertationPath ]

    let data = dirs |> Array.map handleDirectory
    Directory.CreateDirectory(convertationPath) |> ignore
    Directory.CreateDirectory(bfsPath) |> ignore

    for (name, bfsData, convertationData) in data do
        printfn "Successfully processed %s data" name
        let bfsData = sprintf "%s\n%s" name bfsData
        let convertationData = sprintf "%s\n%s" name convertationData
        let name = name + ".data"
        File.WriteAllText(bfsPath + name, bfsData)
        File.WriteAllText(convertationPath + name, convertationData)

    0

exit <| main fsi.CommandLineArgs
