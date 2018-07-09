using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace myCompiler
{

    class DeltaQSigmaGamma
    {
        // структура Delta отображения
        private string LeftQ = null;  // исходное состояние
        private string LeftT = null;  // символ входной цепочки
        private string LeftZ = null;  // верхний символ магазин
        private ArrayList RightQ = null;  // множество следующих состояний
        private ArrayList RightZ = null;  // множество символов магазина

        public string leftQ { get { return LeftQ; } set { LeftQ = value; } }
        public string leftT { get { return LeftT; } set { LeftT = value; } }
        public string leftZ { get { return LeftZ; } set { LeftZ = value; } }
        public ArrayList rightQ { get { return RightQ; } set { RightQ = value; } }
        public ArrayList rightZ { get { return RightZ; } set { RightZ = value; } }

        // Delta (  q1   ,   a    ,   z   ) = {  {q}   ,   {z1z2...} } 
        //         LeftQ    LeftT   LeftZ       RightQ       RightZ 
        public DeltaQSigmaGamma(string LeftQ, string LeftT, string LeftZ, ArrayList RightQ, ArrayList RightZ)
        {
            this.LeftQ = LeftQ;
            this.LeftT = LeftT;
            this.LeftZ = LeftZ;
            this.RightQ = RightQ;
            this.RightZ = RightZ;
        }
    } // end class Delta

    class myMp : Automate    //МП = {}
    {
        // Q - множество состояний МП - автоматa
        // Sigma - алфавит входных символов
        // DeltaList - правила перехода 
        // Q0 - начальное состояние
        // F - множество конечных состояний
        public ArrayList Gamma = null;     //алфавит магазинных символов
        public string z0 = null;           //начальный символ магазина

        public Stack Z = null;
        private int c = 1;

        //
        public myMp(ArrayList Q, ArrayList Sigma, ArrayList Gamma, string Q0, ArrayList F)
            : base(Q, Sigma, F, Q0)
        {
            this.Gamma = Gamma;

            this.Z = new Stack();
            Q0 = Q[0].ToString();  // начальное состояние
            Z.Push(Q0);  // начальный символ в магазине
            this.F = F;       // пустое множество заключительных состояний
        }
        //

        public myMp(myGrammar KCgrammar)
            : base(new ArrayList() { "q" }, KCgrammar.T, new ArrayList() { }, "q")
        {
            this.Gamma = new ArrayList();
            this.Z = new Stack();
            foreach (string v1 in KCgrammar.V)   // магазинные символы
                Gamma.Add(v1);
            foreach (string t1 in KCgrammar.T)
                Gamma.Add(t1);
            Q0 = Q[0].ToString();  // начальное состояние
            Z.Push(KCgrammar.S0);  // начальный символ в магазине
            F = new ArrayList();  // пустое множество заключительных состояний

            DeltaQSigmaGamma delta = null;
            foreach (string v1 in KCgrammar.V)
            {            // сопоставление правил с отображениями
                ArrayList q1 = new ArrayList();
                ArrayList z1 = new ArrayList();
                foreach (Prule rule in KCgrammar.Prules)
                {
                    if (rule.leftNoTerm == v1)
                    {
                        Stack zb = new Stack();
                        ArrayList rr = new ArrayList(rule.rightChain);
                        rr.Reverse();
                        foreach (string s in rr)
                            zb.Push(s);
                        z1.Add(zb);
                        q1.Add(Q0);
                    }
                }
                delta = new DeltaQSigmaGamma(Q0, "e", v1, q1, z1);
                DeltaList.Add(delta);
            }
            foreach (string t1 in KCgrammar.T)
            {
                Stack e = new Stack();
                e.Push("e");
                delta = new DeltaQSigmaGamma(Q0, t1, t1, new ArrayList() { Q0 }, new ArrayList() { e });
                DeltaList.Add(delta);
            }
        }

        public void addDeltaRule(string LeftQ, string LeftT, string LeftZ, ArrayList RightQ, ArrayList RightZ)
        {
            DeltaList.Add(new DeltaQSigmaGamma(LeftQ, LeftT, LeftZ, RightQ, RightZ));
        }

        public bool Execute_(string str)
        {

            string currState = this.Q0;
            DeltaQSigmaGamma delta = null;
            int i = 0;
            str = str + "e";
            for (; ; )  // empty step
            {
                delta = findDelta(currState, str[i].ToString());
                if (delta == null) return false;
                if (delta.leftT != "e")
                {
                    for (; i < str.Length;)
                    {
                        this.Q = delta.rightQ;
                        currState = arrToStr(delta.rightQ);
                        if (delta.leftZ == Z.Peek().ToString() && delta.rightZ[0].ToString() == "e")
                        {
                            this.Z.Pop();
                        }
                        else this.Z.Push(delta.leftT);
                        i++;
                        break;
                    }
                }
                else if (delta.leftT == "e")
                {
                    this.Q = delta.rightQ;
                    this.Z.Pop();
                    if (this.Z.Count == 0) return true;
                    else return false;
                }
            } // end for
        } // end Execute_

        //
        // поиск правила по состоянию. 
        private DeltaQSigmaGamma findDelta(string Q, string a)
        {
            foreach (DeltaQSigmaGamma delta in this.DeltaList)
            {
                if (delta.leftQ == Q && delta.leftT == a) return delta;
            }
            return null; // not find 
        }

        public bool Execute(string s, int i, int j, Stack z0)
        {
            // рекурсивный метод (i-текущий символ в строке, j-текущее состояние, z0-текущие символы в магазине)
            bool k = false;         // успех/неуспех работы
            string a = s.Substring(i, 1);
            string cq = Q[j].ToString();

            if (z0.Count != 0 && z0.Peek().ToString() == this.Q0) c = 1;
            if (i == s.Length)
            {           // если конец строки
                if (z0.Count == 0)
                {        // если магазин пуст
                    if (F.Count == 0) return true;    // множество заключительных состояний пусто 
                    else if (F.Contains(Q[j])) return true;   // или автомат дошел до заключительного состояния
                    else return false;
                }
                else return false;
            }

            Console.WriteLine("\nstack z0 :\n" + DebugStack(z0));
            foreach (DeltaQSigmaGamma delta in this.DeltaList)
            {
                //Console.WriteLine(" delta ");
                if (z0.Count == 0) return true;
                else
                    if (delta.leftQ == cq && (delta.leftT == a || delta.leftT == "e") && delta.leftZ == z0.Peek().ToString())
                {
                    // подходящее правило для текущей конфигурации
                    Stack zz1 = new Stack();   // здесь и далее - создание дополнительных стеков для магазина (предотвращение потери данных)
                    Stack zz2 = new Stack();

                    c++;
                    string zz;
                    while (zz2.Count != 0)
                    {
                        zz = zz2.Pop().ToString();
                        if (zz != "e")
                        {
                            zz1.Push(zz);
                            z0.Push(zz);
                        }
                    }
                    if (zz1.Count != 0) zz1.Pop();
                    foreach (string q1 in delta.rightQ)
                    {
                        j = Q.IndexOf(q1);  // переход в новое состояние
                        foreach (Stack z2 in delta.rightZ)
                        {
                            if (delta.rightQ[delta.rightZ.IndexOf(z2)].ToString() == q1)
                            {   // замена верхнего символа на цепочку
                                Stack zz10 = new Stack();                                   // стек со снятой вершиной zz1 запоминаем в zz10                                    
                                Stack zz20 = new Stack();
                                while (zz1.Count != 0) zz20.Push(zz1.Pop());
                                string zzz;
                                while (zz20.Count != 0)
                                {
                                    zzz = zz20.Pop().ToString();
                                    if (zzz != "e")
                                    {
                                        zz10.Push(zzz);
                                        zz1.Push(zzz);
                                    }
                                }
                                //  замена в стеке zz10
                                Stack z1 = new Stack();
                                while (z2.Count != 0)
                                {
                                    z1.Push(z2.Pop());
                                }
                                string z3;
                                while (z1.Count != 0)
                                {
                                    z3 = z1.Pop().ToString();
                                    if (z3 != "e")
                                    {
                                        zz10.Push(z3);
                                        z2.Push(z3);
                                    }
                                }

                                //     формирование стека z02 для нового такта 

                                Stack z01 = new Stack();
                                Stack z02 = new Stack();

                                while (zz10.Count != 0) z01.Push(zz10.Pop());
                                string z00;
                                while (z01.Count != 0)
                                {
                                    z00 = z01.Pop().ToString();
                                    if (z00 != "e")
                                    {
                                        z02.Push(z00);
                                        zz10.Push(z00);
                                    }
                                }
                                if (delta.leftT == "e") i--;
                                if (Execute(s, i + 1, j, z02)) { k = true; return true; }   // рекурсивный вызов
                                if (delta.leftT == "e") i++;
                            }
                        }
                    }
                }
                if (k) break;
            }
            if (k) return true;
            else return false;
        }

        //*** вспомогательные процедуры ***

        //объединение множеств A or B
        public ArrayList Unify(ArrayList A, ArrayList B)
        {
            ArrayList unify = A;
            foreach (string s in B)
                if (!A.Contains(s))
                    unify.Add(s);
            return unify;
        }

        //преобразование элементов массива в строку
        public string arrToStr(ArrayList array)
        {
            if (array.Equals(null)) return null;
            else
            {
                string newLine = "";
                foreach (string s in array)
                    newLine += s;
                return newLine;
            }
        }

        public string StackToString(Stack Z)
        {
            if (Z.Count == 0) return null;
            else
            {
                string newLine = "";
                Stack temp = new Stack();
                for (int i = 0; i < Z.Count; i++)
                {
                    temp.Push(Z.Pop());
                    newLine += Z.Peek();
                }
                for (int i = 0; i < temp.Count; i++)
                    Z.Push(temp.Pop());
                return newLine;
            }
        }



        // **   Debug   **
        public string DebugStack(Stack s)
        { // печать текущего состояния магазина
            string p = "|";
            Stack s1 = new Stack();
            while (s.Count != 0)
            {
                s1.Push(s.Pop());
                p = p + s1.Peek().ToString();
            }
            while (s1.Count != 0) s.Push(s1.Pop());
            return p;
        }

        public void debugDelta()
        {
            Console.WriteLine("Deltarules :");
            if (this.DeltaList == null) Console.WriteLine("null");
            else
                foreach (DeltaQSigmaGamma d in this.DeltaList)
                { // тут
                    Console.Write("( " + d.leftQ + " , " + d.leftT + " , " + d.leftZ + " )");
                    Console.Write(" -> \n");
                    Console.WriteLine("[ { " + arrToStr(d.rightQ) + " } , { " + arrToStr(d.rightZ) + " } ]");
                }
        }
    }
}
