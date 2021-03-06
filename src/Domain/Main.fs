module Domain.Main
open Types
open System.Collections.Generic

let takeWhilePlusOne predicate (s:seq<_>) =
  let rec loop (en:IEnumerator<_>) = seq {
    if en.MoveNext() then
      yield en.Current
      if predicate en.Current then
        yield! loop en }

  seq { use en = s.GetEnumerator()
        yield! loop en }


let playUntilFinish initialState applyRoll (rollSequence : DiceRoll seq) =
  rollSequence
  |> Seq.scan (fun state inp ->
       state |> (fun state -> (fst state) |> applyRoll  inp)) (initialState, 0)
  |> takeWhilePlusOne (fun (_, finalIndex) -> finalIndex < Constants.fieldsCount)
  |> Seq.last |> (fun (result, _) -> result)

let winner (map : Map) g : Camel =
    let lastNonEmptyStack =
        map
        |> Array.rev
        |> Array.choose (
            function
            | Some (CamelStack []) -> None
            | Some (CamelStack s) -> Some s
            | _ -> None)
        |> Array.head
    lastNonEmptyStack |> List.head


let winnerPercentages totalGames =
    List.map (fun (camel, gamesWon) ->
        (camel, (float gamesWon) / (float totalGames))
    )
    >> List.sortByDescending snd

let winnerCounts rolls map =
    rolls
    |> Seq.map (fun game -> game, playUntilFinish map MoveCamel.applyRoll game)
    |> Seq.map (fun (g, m) -> winner m g)
    |> Seq.countBy id
    |> Seq.toList

let stageWinChances map camelsLeft =
  let rolls = RollSequences.allRollCombinations camelsLeft
  let winnerCounts = winnerCounts rolls map
  let totalGames = winnerCounts |> List.sumBy snd
  winnerPercentages totalGames winnerCounts


let raceWinChances map camelsLeft =
  let rolls =
    Seq.init
      10000
      (fun _ -> RollSequences.infiniteSimulatedRolls camelsLeft)
  let winnerCounts = winnerCounts rolls map
  let totalGames = winnerCounts |> List.sumBy snd
  winnerPercentages totalGames winnerCounts
