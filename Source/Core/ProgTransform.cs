using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using BoogiePL = Microsoft.Boogie;
using System.Diagnostics;

namespace Microsoft.Boogie
{
  
  public class ProgTransformer : Duplicator
  {
    protected Program program;

    public ProgTransformer(Program program)
    {
      this.program = program;
    }

    protected static void ProcessImplementation(Program program, Implementation impl, ProgTransformer progTransformer)
    {
      Contract.Requires(impl != null);
      Contract.Requires(impl.Proc != null);


      // we need to resolve the new code
      progTransformer.ResolveImpl(impl);

      // if (CommandLineOptions.Clo.PrintInlined)
      {
        progTransformer.EmitImpl(impl);
      }
    }

    public static void ProcessImplementation(Program program, Implementation impl)
    {
      Contract.Requires(impl != null);
      Contract.Requires(program != null);
      Contract.Requires(impl.Proc != null);
      ProcessImplementation(program, impl, new ProgTransformer(program));
    }
    
    private sealed class DummyErrorSink : IErrorSink
    {
      public void Error(IToken tok, string msg)
      {
        //Contract.Requires(msg != null);
        //Contract.Requires(tok != null);
        // FIXME 
        // noop.
        // This is required because during the resolution, some resolution errors happen
        // (such as the ones caused addion of loop invariants J_(block.Label) by the AI package
      }
    }

    
    protected void ResolveImpl(Implementation impl)
    {
      Contract.Requires(impl != null);
      Contract.Ensures(impl.Proc != null);
      ResolutionContext rc = new ResolutionContext(new DummyErrorSink());

      foreach (var decl in program.TopLevelDeclarations)
      {
        decl.Register(rc);
      }

      impl.Proc = null; // to force Resolve() redo the operation
      impl.Resolve(rc);

      TypecheckingContext tc = new TypecheckingContext(new DummyErrorSink());

      impl.Typecheck(tc);
    }

    protected void EmitImpl(Implementation impl)
    {
      Contract.Requires(impl != null);
      Contract.Requires(impl.Proc != null);
      Console.WriteLine("after tranforming the programs calls");
      impl.Proc.Emit(new TokenTextWriter("<console>", Console.Out, /*pretty=*/ false), 0);
      impl.Emit(new TokenTextWriter("<console>", Console.Out, /*pretty=*/ false), 0);
    }

  }
}