#!/usr/bin/env -S dotnet fsi

open System
open System.IO

let resultsPath = "experiments/results_lagraph/"
let convertationPath = "experiments/data/convertation_data_lagraph/"
let bfsPath = "experiments/data/bfs_data_lagraph/"

let convertationLine = 2

let getTime (str: string) =
    match str with
    | null -> "0.000"
    | _ ->
        try
            // Look for "Load time: X.XXX s" or "BFS time: X.XXX s"
            if str.Contains("Load time:") then
                let idx = str.IndexOf("Load time:")
                let rest = str.Substring(idx + 10).Trim()
                let parts = rest.Split(' ')
                parts.[0]
            elif str.Contains("BFS time:") then
                let idx = str.IndexOf("BFS time:")
                let rest = str.Substring(idx + 9).Trim()
                let parts = rest.Split(' ')
                parts.[0]
            else
                "0.000"
        with
        | _ -> "0.000"

let getThreads (str: string) =
    try
        // Look for "X threads" in the line - format: "(load matrix, X threads, ..."
        let idx = str.IndexOf("threads")
        if idx > 0 then
            let before = str.Substring(0, idx).Trim()
            let commaIdx = before.LastIndexOf(',')
            if commaIdx > 0 then
                let numStr = before.Substring(commaIdx + 1).Trim()
                int numStr
            else
                1
        else
            1
    with
    | _ -> 1

let handleFile filePath =
    try
        let text = File.ReadAllLines(filePath)
        // Line 5: Load time, Line 6: BFS time
        if text.Length >= 6 then
            let loadTime = getTime text.[4]
            let bfsTime = getTime text.[5]
            let threads = getThreads text.[1]
            Some (threads, loadTime, bfsTime)
        else
            None
    with
    | _ -> None

let handleDirectory dir =
    let files = Directory.GetFiles(dir, "*.output")
    let data = 
        files 
        |> Array.choose handleFile 
        |> Array.sortBy (fun (threads, _, _) -> threads)
        |> Array.rev

    let convertationDataFile =
        let arr = data |> Array.map (fun (threads, loadTime, _) -> sprintf "%d %s" threads loadTime)
        String.concat "\n" arr

    let bfsDataFile =
        let arr = data |> Array.map (fun (threads, _, bfsTime) -> sprintf "%d %s" threads bfsTime)
        String.concat "\n" arr

    let name = DirectoryInfo(dir).Name
    (name, bfsDataFile, convertationDataFile)

let main args =
    if not (Directory.Exists(resultsPath)) then
        printfn "Error: results directory '%s' not found" resultsPath
        exit 1

    let dirs = Directory.GetDirectories(resultsPath)
    
    if dirs.Length = 0 then
        printfn "No result directories found in %s" resultsPath
        exit 0

    Directory.CreateDirectory(convertationPath) |> ignore
    Directory.CreateDirectory(bfsPath) |> ignore

    let data = dirs |> Array.map handleDirectory

    for (name, bfsData, convertationData) in data do
        printfn "Successfully processed %s data" name
        let bfsDataStr = sprintf "%s\n%s" name bfsData
        let convertationDataStr = sprintf "%s\n%s" name convertationData
        let fileName = name + ".data"
        File.WriteAllText(bfsPath + fileName, bfsDataStr)
        File.WriteAllText(convertationPath + fileName, convertationDataStr)

    0

exit <| main fsi.CommandLineArgs
