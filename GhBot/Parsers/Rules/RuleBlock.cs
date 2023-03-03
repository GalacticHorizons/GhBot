using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace GhBot.Parsers.Rules;

public struct RuleBlock
{
    public string Title;
    public string Color;
    public string Description;

    public Rule[] Rules;

    public static RuleBlock[] Parse(string text)
    {
        string[] splitText = text.Split('\n');

        bool inBlock = false;
        TextType currentText = TextType.None;
        string tempText = "";
        RuleBlock block = new RuleBlock();
        Rule rule = new Rule();
        List<RuleBlock> blocks = new List<RuleBlock>();
        List<Rule> rules = new List<Rule>();

        for (int l = 0; l < splitText.Length; l++)
        {
            string line = splitText[l];

            if (!inBlock && l < splitText.Length - 1)
            {
                bool isGood = true;
                
                int length = 0;
                foreach (char c in splitText[l + 1])
                {
                    if (c != '-')
                    {
                        isGood = false;
                        break;
                    }

                    length++;
                }

                if (length != line.Length && length != splitText[l + 1].Length)
                    isGood = false;

                if (isGood)
                {
                    block.Title = line;
                    inBlock = true;
                    l++;
                    continue;
                }
            }

            //if (!inBlock)
            //    throw new Exception("Empty rule block not allowed.");
            
            if (string.IsNullOrWhiteSpace(line))
            {
                block.Rules = rules.ToArray();
                rules.Clear();
                blocks.Add(block);
                block = new RuleBlock();
                inBlock = false;
                currentText = TextType.None;
                continue;
            }

            /*bool isDash = line[0] == '-';
            bool isNum = line[0] is >= '1' and <= '9' && line[1] == '.';
            // It must be a list!
            if (isDash || isNum)
            {
                if (currentText != TextType.None)
                {
                    switch (currentText)
                    {
                        case TextType.Description:
                            block.Description = tempText;
                            break;
                        case TextType.Rule:
                            rule.Description = tempText;
                            rules.Add(rule);
                            rule = new Rule();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    tempText = "";
                }
                
                if (isDash)
                    rule.Title = line[1..].Trim(' ');
                //else if (isNum)
                //    rule.Title = line;

                currentText = TextType.Rule;
            }

            if (currentText != TextType.None)
                tempText += line;*/
        }

        return blocks.ToArray();
    }

    private enum TextType
    {
        None,
        Description,
        Rule
    }
}