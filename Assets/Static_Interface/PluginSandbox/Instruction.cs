using System.Reflection.Emit;

namespace Static_Interface.PluginSandbox
{
    public class Instruction
    {
        public OpCode OpCode;
        public object Operand;
        public long Offset;
        public long LocalVariableIndex;
    }
}