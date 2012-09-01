namespace Microsoft.FSharp.Compiler

// --------------------------------------------------------------------------------------

/// Implements the (?) operator that makes it possible to access internal methods
/// and properties and contains definitions for F# assemblies
module internal Reflection =
  open System
  open System.Reflection
  open Microsoft.FSharp.Reflection
 
  let staticMethodFlags = BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Static
  let instanceMethodFlags = BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance
  let ctorFlags = instanceMethodFlags
  let inline asMethodBase(a:#MethodBase) = a :> MethodBase
  
  let (?) (o:obj) name : 'R =
    // The return type is a function, which means that we want to invoke a method
    if FSharpType.IsFunction(typeof<'R>) then
      let argType, resType = FSharpType.GetFunctionElements(typeof<'R>)
      FSharpValue.MakeFunction(typeof<'R>, fun args ->
        // We treat elements of a tuple passed as argument as a list of arguments
        // When the 'o' object is 'System.Type', we call static methods
        let methods, instance, args =
          let args =
            if argType = typeof<unit> then [| |]
            elif not(FSharpType.IsTuple(argType)) then [| args |]
            else FSharpValue.GetTupleFields(args)
          if (typeof<System.Type>).IsAssignableFrom(o.GetType()) then
            let methods = (unbox<Type> o).GetMethods(staticMethodFlags) |> Array.map asMethodBase
            let ctors = (unbox<Type> o).GetConstructors(ctorFlags) |> Array.map asMethodBase
            Array.concat [ methods; ctors ], null, args
          else
            o.GetType().GetMethods(instanceMethodFlags) |> Array.map asMethodBase, o, args
        
        // A simple overload resolution based on the name and number of parameters only
        let methods =
          [ for m in methods do
              if m.Name = name && m.GetParameters().Length = args.Length then yield m ]
        match methods with
        | [] -> failwithf "No method '%s' with %d arguments found" name args.Length
        | _::_::_ -> failwithf "Multiple methods '%s' with %d arguments found" name args.Length
        | [:? ConstructorInfo as c] -> c.Invoke(args)
        | [ m ] -> m.Invoke(instance, args) ) |> unbox<'R>
    else
      // When the 'o' object is 'System.Type', we access static properties
      let typ, flags, instance =
        if (typeof<System.Type>).IsAssignableFrom(o.GetType()) then unbox o, BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Static, null
        else o.GetType(), BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance, o
      
      // Find a property that we can call and get the value
      let prop = typ.GetProperty(name, flags)
      if prop = null then failwithf "Property '%s' not found in '%s' using flags '%A'." name typ.Name flags
      let meth = prop.GetGetMethod(true)
      if prop = null then failwithf "Property '%s' found, but doesn't have 'get' method." name
      meth.Invoke(instance, [| |]) |> unbox<'R>


  /// Wrapper type for the 'FSharp.Compiler.dll' assembly - expose types we use
  type FSharpCompiler private () =
    static let asm = Assembly.Load "FSharp.Compiler"
    static member InteractiveChecker = asm.GetType "Microsoft.FSharp.Compiler.SourceCodeServices.InteractiveChecker"
    static member IsResultObsolete = asm.GetType "Microsoft.FSharp.Compiler.SourceCodeServices.IsResultObsolete"
    static member CheckOptions = asm.GetType "Microsoft.FSharp.Compiler.SourceCodeServices.CheckOptions"
    static member SourceTokenizer = asm.GetType "Microsoft.FSharp.Compiler.SourceCodeServices.SourceTokenizer"

  /// Wrapper type for the 'FSharp.Compiler.Server.Shared.dll' assembly - expose types we use
  type FSharpCompilerServerShared private () =
    static let asm = Assembly.Load "FSharp.Compiler.Server.Shared"
    static member InteractiveServer = asm.GetType "Microsoft.FSharp.Compiler.Server.Shared.FSharpInteractiveServer"

    
open System.Globalization

module internal Server =
  module Shared =
    open Reflection
    
    type FSharpInteractiveServer(wrapped:obj) =     
      static member StartClient(channel:string) =
        FSharpInteractiveServer
          (FSharpCompilerServerShared.InteractiveServer?StartClient channel)

      member x.Interrupt() : unit = wrapped?Interrupt()
      member x.Completions (prefix: string): (Server.Shared.CompletionType * string)[] = wrapped?Completions prefix
