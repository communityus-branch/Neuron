using UnityEditor;
using SyntaxTree.VisualStudio.Unity.Bridge;

[InitializeOnLoad]
public class CSharp60Enforcer
{

    static CSharp60Enforcer()
    {
        ProjectFilesGenerator.ProjectFileGeneration += (string name, string content) =>
        {
            string format = "<LangVersion Condition=\" '$(VisualStudioVersion)' != '10.0' \">%s</LangVersion>";
            return content.Replace(string.Format(format, "4"), string.Format(format, "6"));
        };
    }

}