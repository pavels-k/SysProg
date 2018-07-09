﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace myCompiler
{

    class Prule : Rule
    {
        public static int Count = 0;
        public int Id;
        public ArrayList RightChain;

        public Prule(string LeftNoTerm, ArrayList RightChain)
            : base(LeftNoTerm)
        {
            Count++;
            Id = Count;
            this.RightChain = RightChain;
        }

        public ArrayList rightChain
        {
            get { return RightChain; }
            set { RightChain = value; }
        }
    }//end Prule

    class myGrammar : Grammar
    {
        public myGrammar(ArrayList T, ArrayList V, string S0)
            : base(T, V, S0) { Prule.Count = 0; this.Prules = new ArrayList(); }

        public myGrammar(ArrayList T, ArrayList V, ArrayList Prules, string S0)
            : base(T, V, S0) { Prule.Count = 0; this.Prules = Prules; }

        public myGrammar() : base() { Prule.Count = 0; }

        //порождение цепочек символов по правилам вывода
        public override string Execute()
        {
            string bornedLine = null;
            string currState = "S0";
            foreach (Prule p in this.Prules)
            {
                if (p.leftNoTerm == currState)
                {
                    //
                }
            }
            return bornedLine;
        }

    }// end myGrammar
}
