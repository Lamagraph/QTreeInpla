let getBigCoo (linewords: string array array) =
    linewords
    |> Array.map (fun x -> ((int x.[0]) - 1), ((int x.[1]) - 1))
    |> Array.map (fun (i, j) -> sprintf "(%d, %d, 1)" i j)
    |> String.concat ", "
    |> sprintf "[%s]"

let getBigCooDuplicate (linewords: string array array) =
    linewords
    |> Array.map (fun x -> ((int x.[0]) - 1), ((int x.[1]) - 1))
    |> Array.map (fun (i, j) -> sprintf "(%d, %d, 1), (%d, %d, 1)" i j j i)
    |> String.concat ", "
    |> sprintf "[%s]"
