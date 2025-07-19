using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using HapticLibrary.Models;

public class F451Serializer
{
    AudioBookJson F451Json = new AudioBookJson
    {
        Title = "Fahrenheit 451",
        Audio = new AudioMetadata
        {
            Src = "D:\\Computer Science\\DataFeel\\HapticLibrary\\HapticLibrary\\Assets\\F451.wav",
            Duration = 122.0
        },
        Pages = new List<AudioPage>
        {
            new AudioPage
            {
                    PageNumber = 1,
                    StartTime = 0,
                    EndTime = 205,
                    Text = "\"Police suggest entire population in the Elm Terrace area do as follows: Everyone in every house in every street open a front or rear door or look from the windows. \n\nThe fugitive cannot escape if everyone in the next minute looks from his house. Ready! \"\n\nOf course! Why hadn't they done it before! Why, in all the years, hadn't this game been tried!"
            },
            new AudioPage
            {
                PageNumber = 2,
                StartTime = 206,
                EndTime = 405,
                Text = "Everyone up, everyone out! He couldn't be missed! The only man running alone in the night city, the only man proving his legs!\n\n\"At the count of ten now! One! Two!\" He felt the city rise. Three .\n\nHe felt the city turn to its thousands of doors. Faster! Leg up, leg down !"
            },
            new AudioPage
            {
                PageNumber = 3,
                StartTime = 406,
                EndTime = 593,
                Text = "\"Four !\" The people sleepwalking in their hallways. \"Five! \" He felt their hands on the doorknobs!\n\nThe smell of the river was cool and like a solid rain. His throat was burnt rust and his eyes were wept dry with running. \n\nHe yelled as if this yell would jet him on, fling him the last hundred yards. \"Six, seven, eight !\""
            },
            new AudioPage
            {
                PageNumber = 4,
                StartTime = 594,
                EndTime = 910,
                Text = "The doorknobs turned on five thousand doors. \"Nine!\" He ran out away from the last row of houses, on a slope leading down to a solid moving blackness. \"Ten!\" \n\nThe doors opened. He imagined thousands on thousands of faces peering into yards, into alleys, and into the sky, faces hid by curtains, pale, night-frightened faces, like grey animals peering from electric caves, faces with grey colourless eyes, grey tongues and grey thoughts looking out through the numb flesh of the face.\n\nBut he was at the river."
            },
            new AudioPage
            {
                PageNumber = 5,
                StartTime = 911,
                EndTime = 1220,
                Text = "He touched it, just to be sure it was real. He waded in and stripped in darkness to the skin, splashed his body, arms, legs, and head with raw liquor; drank it and snuffed some up his nose. \n\nThen he dressed in Faber's old clothes and shoes.  He tossed his own clothing into the river and watched it swept away. \n\nThen, holding the suitcase, he walked out in the river until there was no bottom and he was swept away in the dark."
            }
        },
        Effects = new List<Effect>
        {
            new Effect
            {
                Type = "Event1",
                StartTime = 1,
                EndTime = 160,
                //Dots = null     // TODO: add Dots with all of their sensations
            },
            new Effect
            {
                Type = "Event2",
                StartTime = 170,
                EndTime = 320,
                //Dots = null     // TODO: add Dots with all of their sensations
            },
            new Effect
            {
                Type = "Event3",
                StartTime = 308,
                EndTime = 685,
                //Dots = null     // TODO: add Dots with all of their sensations
            },
            new Effect
            {
                Type = "Event4",
                StartTime = 730,
                EndTime = 920,
                //Dots = null     // TODO: add Dots with all of their sensations
            },
            new Effect
            {
                Type = "Event5",
                StartTime = 924,
                EndTime = 1124,
                //Dots = null     // TODO: add Dots with all of their sensations
            },
            new Effect
            {
                Type = "Event6",
                StartTime = 1146,
                EndTime = 1220,
                //Dots = null     // TODO: add Dots with all of their sensations
            }
        }
    }; 
}

