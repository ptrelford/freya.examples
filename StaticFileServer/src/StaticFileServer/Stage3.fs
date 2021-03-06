﻿module StaticFileServer.Stage3

open System.IO
open Freya.Core
open Freya.Core.Operators
open Freya.Lenses.Http
open Freya.Machine
open Freya.Machine.Extensions.Http

let getFileInfo (path: string) =
    let filePath = Path.Combine (root.FullName, path.Trim ([| '/' |]))
    let fileInfo = FileInfo (filePath)

    fileInfo

let readFile (file: FileInfo) =
    File.ReadAllBytes (file.FullName)

// Response

let represent _ x =
    { Description =
        { Charset = None
          Encodings = None
          MediaType = None
          Languages= None }
      Data = x }

// Freya

let path =
    Freya.memo (Freya.Lens.get Request.Path_)

let fileInfo =
    Freya.memo (getFileInfo <!> path)

let file =
    Freya.memo (readFile <!> fileInfo)

// Machine

let existsDecision =
    (fun (x: FileInfo) -> x.Exists) <!> fileInfo

let fileHandler n =
    represent n <!> file

// Resources

let files : FreyaPipeline =
    freyaMachine {
        including defaults
        exists existsDecision
        handleOk fileHandler } |> FreyaMachine.toPipeline