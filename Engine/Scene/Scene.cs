using System.Collections.Generic;

namespace _3Dvishok.Engine.Scene
{
    public class Scene
    {
        public List<Model> Models { get; private set; }

        public Scene()
        {
            Models = new List<Model>();
        }

        public void Add(Model model)
        {
            Models.Add(model);
        }

        public bool Remove(Model model)
        {
            return Models.Remove(model);
        }

        public void Clear()
        {
            Models.Clear();
        }
    }
}
