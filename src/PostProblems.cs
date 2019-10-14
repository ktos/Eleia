using System;
using System.Collections.Generic;
using System.Text;

namespace Eleia
{
    /// <summary>
    /// Represents any possible problem with a post, like unformatted code,
    /// bad title, bad tags and so on
    /// </summary>
    public abstract class PostProblems
    {
        public float Probability { get; set; }
    }

    /// <summary>
    /// Represents that a class has a not formatted code somewhere
    /// </summary>
    public class NotFormattedCodeFound : PostProblems
    {
        public override string ToString()
        {
            return $"Potentially not formatted code found (prob: {Probability})";
        }
    }
}