using System.Collections;
using System.Collections.Generic;

namespace AdvancedSceneManager.Models.Interfaces
{

    /// <summary>Defines some core properties for scene collections.</summary>
    public interface ISceneCollection : IASMModel, IEnumerable<Scene>, IEnumerable
    {

        /// <summary>Gets the scenes of this collection.</summary>
        public IEnumerable<Scene> scenes { get; }

        /// <summary>Gets the scenes of this collection.</summary>
        public IEnumerable<string> scenePaths { get; }

        /// <summary>Gets the title of this collection.</summary>
        public string title { get; }

        /// <summary>Gets the description of this collection.</summary>
        public string description { get; }

        /// <summary>Gets the scene count of this collection.</summary>
        public int count { get; }

        /// <summary>Gets if this collection contains <paramref name="scene"/>.</summary>
        public bool Contains(Scene scene);

        /// <summary>Gets the scene at the specified index.</summary>
        public Scene this[int index] { get; }

    }

}
