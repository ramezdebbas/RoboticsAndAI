using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace EcommFashion.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : EcommFashion.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Nivax Data");

            var group1 = new SampleDataGroup("Group-1",
                    "Robotics",
                    "Group Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Introduction",
                    "",
                    "Assets/HubPage/HubPage1.png",
                    "Robotics is the branch of technology that deals with the design, construction, operation, and application of robots, as well as computer systems for their control, sensory feedback, and information processing. These technologies deal with automated machines that can take the place of humans in dangerous environments or manufacturing processes, or resemble humans in appearance, behavior, and/or cognition. Many of today's robots are inspired by nature contributing to the field of bio-inspired robotics.\n\nThe concept of creating machines that can operate autonomously dates back to classical times, but research into the functionality and potential uses of robots did not grow substantially until the 20th century. Throughout history, robotics has been often seen to mimic human behavior, and often manage tasks in a similar fashion. Today, robotics is a rapidly growing field, as technological advances continue, research, design, and building new robots serve various practical purposes, whether domestically, commercially, or militarily. Many robots do jobs that are hazardous to people such as defusing bombs, exploring shipwrecks, and mines.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                   "Etymology",
                    "",
                    "Assets/HubPage/HubPage2.png",
                    "The word robotics was derived from the word robot, which was introduced to the public by Czech writer Karel Čapek in his play R.U.R. (Rossum's Universal Robots), which was published in 1920. The word robot comes from the Slavic word robota, which means labor. The play begins in a factory that makes artificial people called robots, creatures who can be mistaken for humans – similar to the modern ideas of androids. Karel Čapek himself did not coin the word. He wrote a short letter in reference to an etymology in the Oxford English Dictionary in which he named his brother Josef Čapek as its actual originator.\n\nAccording to the Oxford English Dictionary, the word robotics was first used in print by Isaac Asimov, in his science fiction short story Liar!, published in May 1941 in Astounding Science Fiction. Asimov was unaware that he was coining the term; since the science and technology of electrical devices is electronics, he assumed robotics already referred to the science and technology of robots. In some of Asimov's other works, he states that the first use of the word robotics was in his short story Runaround (Astounding Science Fiction, March 1942). However, the original publication of Liar! predates that of Runaround by five months, so the former is generally cited as the word's origin.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Three Laws of Robotics",
                    "",
                    "Assets/HubPage/HubPage3.png",
                    "The Three Laws of Robotics (often shortened to The Three Laws or Three Laws) are a set of rules devised by the science fiction author Isaac Asimov. The rules were introduced in his 1942 short story Runaround, although they had been foreshadowed in a few earlier stories. The Three Laws are:\n\nA robot may not injure a human being or, through inaction, allow a human being to come to harm.\nA robot must obey the orders given to it by human beings, except where such orders would conflict with the First Law.\nA robot must protect its own existence as long as such protection does not conflict with the First or Second Laws.\n\nThese form an organizing principle and unifying theme for Asimov's robotic-based fiction, appearing in his Robot series, the stories linked to it, and his Lucky Starr series of young-adult fiction. The Laws are incorporated into almost all of the positronic robots appearing in his fiction, and cannot be bypassed, being intended as a safety feature. Many of Asimov's robot-focused stories involve robots behaving in unusual and counter-intuitive ways as an unintended consequence of how the robot applies the Three Laws to the situation in which it finds itself. Other authors working in Asimov's fictional universe have adopted them and references, often parodic, appear throughout science fiction as well as in other genres.\n\nThis cover of I, Robot illustrates the story Runaround, the first to list all Three Laws of Robotics.\n\nThe original laws have been altered and elaborated on by Asimov and other authors. Asimov himself made slight modifications to the first three in various books and short stories to further develop how robots would interact with humans and each other. In later fiction where robots had taken responsibility for government of whole planets and human civilizations, Asimov also added a fourth, or zeroth law, to precede the others:\nA robot may not harm humanity, or, by inaction, allow humanity to come to harm. The Three Laws, and the zeroth, have pervaded science fiction and are referred to in many books, films, and other media.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "I, Robot",
                    "",
                    "Assets/HubPage/HubPage4.png",
                    "I, Robot is a collection of nine science fiction short stories by Isaac Asimov, first published by Gnome Press in 1950 in an edition of 5,000 copies. The stories originally appeared in the American magazines Super Science Stories and Astounding Science Fiction between 1940 and 1950. The stories are woven together as Dr. Susan Calvin tells them to a reporter (the narrator) in the 21st century. Though the stories can be read separately, they share a theme of the interaction of humans, robots, and morality, and when combined they tell a larger story of Asimov's fictional history of robotics.\n\nSeveral of the stories feature the character of Dr. Susan Calvin, chief robopsychologist at U.S. Robots and Mechanical Men, Inc., the major manufacturer of robots. Upon their publication in this collection, Asimov wrote a framing sequence presenting the stories as Calvin's reminiscences during an interview with her about her life's work, chiefly concerned with aberrant behaviour of robots, and the use of robopsychology to sort them out. The book also contains the short story in which Asimov's Three Laws of Robotics first appear. Other characters that appear in these short stories are Powell and Donovan, a field-testing team which locates flaws in USRMM's prototype models.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Maschinenmensch",
                    "",
                    "Assets/HubPage/HubPage5.png",
                    "The Maschinenmensch (German for machine-human) from Metropolis, is a gynoid (female robot and female android) played by German actress Brigitte Helm in both its robot form and human incarnation. Named Maria in the film, and Futura in the Novel, she was the first robot ever depicted in cinema. Robot Maria's haunting blank face, slightly open lips, and pronounced female curves in the film have been the subject of disgust and fascination alike.\n\nThe Maschinenmensch has been given several names through the decades: Parody, Ultima, Machina, Futura, Robotrix, False Maria, Robot Maria, and Hel. The intertitles of the 2010 restoration of Metropolis quotes Rotwang, the robot's creator, referring to his gynoid Maschinenmensch as the Machine-Man.\n\nThe Maschinenmensch's back story is detailed in Thea von Harbou's original 1927 novel. It is described as a very delicate, but faceless, transparent figure made of crystal flesh with silver bones and its eyes filled with an expression of calm madness. Rotwang addresses it as Parody. When Fredersen asks what it is, he calls her Futura ... Parody whatever you like to call it. Also: Delusion ... In short a woman. Rotwang then explains that Futura is perfectly obedient and that she is the ideal agent-provocateur, able to become any woman and tempt men to their doom. Later, when Rotwang has given it Maria's appearance he instructs her to disobey Fredersen on purpose and foil his plans and ultimately destroy him. Though mention is made of Rotwang's former lover, Hel, they are never directly associated with each other.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                    "Humanoid robot",
                    "",
                    "Assets/HubPage/HubPage6.png",
                    "A humanoid robot is a robot with its body shape built to resemble that of the human body. A humanoid design might be for functional purposes, such as interacting with human tools and environments, for experimental purposes, such as the study of bipedal locomotion, or for other purposes. In general, humanoid robots have a torso, a head, two arms, and two legs, though some forms of humanoid robots may model only part of the body, for example, from the waist up. Some humanoid robots may also have heads designed to replicate human facial features such as eyes and mouths. Androids are humanoid robots built to aesthetically resemble humans.\n\nHumanoid robots are used as a research tool in several scientific areas.Researchers need to understand the human body structure and behavior (biomechanics) to build and study humanoid robots. On the other side, the attempt to simulate the human body leads to a better understanding of it. Human cognition is a field of study which is focused on how humans learn from sensory information in order to acquire perceptual and motor skills. This knowledge is used to develop computational models of human behavior and it has been improving over time.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);

             var group2 = new SampleDataGroup("Group-2",
                    "Artificial Intelligence",
                    "Group Subtitle: 2",
                    "Assets/LightGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
					"Introduction",
					"",
                    "Assets/HubPage/HubPage7.png",
                    "Artificial intelligence (AI) is the intelligence of machines or software, and is also a branch of computer science that studies and develops intelligent machines and software. Major AI researchers and textbooks define the field as the study and design of intelligent agents, where an intelligent agent is a system that perceives its environment and takes actions that maximize its chances of success. John McCarthy, who coined the term in 1955, defines it as the science and engineering of making intelligent machines.AI research is highly technical and specialised, deeply divided into subfields that often fail to communicate with each other. Some of the division is due to social and cultural factors: subfields have grown up around particular institutions and the work of individual researchers. AI research is also divided by several technical issues. There are subfields which are focused on the solution of specific problems, on one of several possible approaches, on the use of widely differing tools and towards the accomplishment of particular applications.\n\nThe central problems (or goals) of AI research include reasoning, knowledge, planning, learning, communication, perception and the ability to move and manipulate objects.General intelligence (or strong AI) is still among the field's long term goals. Currently popular approaches include statistical methods, computational intelligence and traditional symbolic AI. There are an enormous number of tools used in AI, including versions of search and mathematical optimization, logic, methods based on probability and economics, and many others.\n\nThe field was founded on the claim that a central property of humans, intelligence—the sapience of Homo sapiens—can be so precisely described that it can be simulated by a machine. This raises philosophical issues about the nature of the mind and the ethics of creating artificial beings, issues which have been addressed by myth, fiction and philosophy since antiquity. Artificial intelligence has been the subject of tremendous optimism but has also suffered stunning setbacks. Today it has become an essential part of the technology industry, providing the heavy lifting for many of the most difficult problems in computer science.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Knowledge representation",
                    "",
                    "Assets/HubPage/HubPage8.png",
                    "Knowledge representation and knowledge engineering are central to AI research. Many of the problems machines are expected to solve will require extensive knowledge about the world. Among the things that AI needs to represent are: objects, properties, categories and relations between objects; situations, events, states and time; causes and effects; knowledge about knowledge (what we know about what other people know); and many other, less well researched domains. A representation of what exists is an ontology: the set of objects, relations, concepts and so on that the machine knows about. The most general are called upper ontologies, which attempt to provide a foundation for all other knowledge.Among the most difficult problems in knowledge representation are:\n\nDefault reasoning and the qualification problem\n\nMany of the things people know take the form of working assumptions. For example, if a bird comes up in conversation, people typically picture an animal that is fist sized, sings, and flies. None of these things are true about all birds. John McCarthy identified this problem in 1969 as the qualification problem: for any commonsense rule that AI researchers care to represent, there tend to be a huge number of exceptions. Almost nothing is simply true or false in the way that abstract logic requires. AI research has explored a number of solutions to this problem. The breadth of commonsense knowledge \n\nThe number of atomic facts that the average person knows is astronomical. Research projects that attempt to build a complete knowledge base of commonsense knowledge (e.g., Cyc) require enormous amounts of laborious ontological engineering — they must be built, by hand, one complicated concept at a time. A major goal is to have the computer understand enough concepts to be able to learn by reading from sources like the internet, and thus be able to add to its own ontology.\n\nThe subsymbolic form of some commonsense knowledge \n\nMuch of what people know is not represented as facts or statements that they could express verbally. For example, a chess master will avoid a particular chess position because it feels too exposed or an art critic can take one look at a statue and instantly realize that it is a fake. These are intuitions or tendencies that are represented in the brain non-consciously and sub-symbolically. Knowledge like this informs, supports and provides a context for symbolic, conscious knowledge. As with the related problem of sub-symbolic reasoning, it is hoped that situated AI, computational intelligence, or statistical AI will provide ways to represent this kind of knowledge.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                     "Planning",
                    "",
                    "Assets/HubPage/HubPage9.png",
                    "Intelligent agents must be able to set goals and achieve them. They need a way to visualize the future (they must have a representation of the state of the world and be able to make predictions about how their actions will change it) and be able to make choices that maximize the utility (or value) of the available choices.\nIn classical planning problems, the agent can assume that it is nthe only thing acting on the world and it can be certain what the consequences of its actions may be. However, if the agent is not the only actor, it must periodically ascertain whether the world matches its predictions and it must change its plan as this becomes necessary, requiring the agent to reason under uncertainty.\n\nMulti-agent planning uses the cooperation and competition of many agents to achieve a given goal. Emergent behavior such as this is used by evolutionary algorithms and swarm intelligence.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                    "Learning",
                   "",
                   "Assets/HubPage/HubPage10.png",
                   "Machine learning, a branch of artificial intelligence, is about the construction and study of systems that can learn from data. For example, a machine learning system could be trained on email messages to learn to distinguish between spam and non-spam messages. After learning, it can then be used to classify new email messages into spam and non-spam folders.\n\nThe core of machine learning deals with representation and generalization. Representation of data instances and functions evaluated on these instances are part of all machine learning systems. Generalization is the property that the system will perform well on unseen data instances; the conditions under which this can be guaranteed are a key object of study in the subfield of computational learning theory. There is a wide variety of machine learning tasks and successful applications. Optical character recognition, in which printed characters are recognized automatically based on previous examples, is a classic example of machine learning.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-5",
                    "Natural language processing",
                   "",
                   "Assets/HubPage/HubPage11.png",
                   "Modern NLP algorithms are based on machine learning, especially statistical machine learning. The paradigm of machine learning is different from that of most prior attempts at language processing. Prior implementations of language-processing tasks typically involved the direct hand coding of large sets of rules. The machine-learning paradigm calls instead for using general learning algorithms — often, although not always, grounded in statistical inference — to automatically learn such rules through the analysis of large corpora of typical real-world examples. A corpus (plural, corpora) is a set of documents (or sometimes, individual sentences) that have been hand-annotated with the correct values to be learned.\n\nMany different classes of machine learning algorithms have been applied to NLP tasks. These algorithms take as input a large set of features that are generated from the input data. Some of the earliest-used algorithms, such as decision trees, produced systems of hard if-then rules similar to the systems of hand-written rules that were then common. Increasingly, however, research has focused on statistical models, which make soft, probabilistic decisions based on attaching real-valued weights to each input feature. Such models have the advantage that they can express the relative certainty of many different possible answers rather than only one, producing more reliable results when such a model is included as a component of a larger system.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-6",
                    "Computer vision",
                   "",
                   "Assets/HubPage/HubPage12.png",
                   "Computer vision is a field that includes methods for acquiring, processing, analyzing, and understanding images and, in general, high-dimensional data from the real world in order to produce numerical or symbolic information, e.g., in the forms of decisions. A theme in the development of this field has been to duplicate the abilities of human vision by electronically perceiving and understanding an image. This image understanding can be seen as the disentangling of symbolic information from image data using models constructed with the aid of geometry, physics, statistics, and learning theory. Computer vision has also been described as the enterprise of automating and integrating a wide range of processes and representations for vision perception.\n\nApplications range from tasks such as industrial machine vision systems which, say, inspect bottles speeding by on a production line, to research into artificial intelligence and computers or robots that can comprehend the world around them. The computer vision and machine vision fields have significant overlap. Computer vision covers the core technology of automated image analysis which is used in many fields. Machine vision usually refers to a process of combining automated image analysis with other methods and technologies to provide automated inspection and robot guidance in industrial applications.\n\nAs a scientific discipline, computer vision is concerned with the theory behind artificial systems that extract information from images. The image data can take many forms, such as video sequences, views from multiple cameras, or multi-dimensional data from a medical scanner.",
                   ITEM_CONTENT,
                   group2));
            this.AllGroups.Add(group2); 


        }
    }
}
