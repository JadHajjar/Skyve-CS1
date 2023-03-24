namespace LoadOrderInjections {
    using System;
    using System.Collections.Generic;
    using Mono.Cecil;

    public class MyAssemblyResolver : BaseAssemblyResolver {
        public static MyAssemblyResolver CreateDefault() {
            var r = new MyAssemblyResolver();
            var readerParameters = new ReaderParameters {
                ReadWrite = false,
                InMemory = true,
                AssemblyResolver = r,
            };
            r.ReaderParameters = readerParameters;
            return r;
        }

        private readonly IDictionary<string, AssemblyDefinition> cache
            = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);

        private ReaderParameters readerParameters_;
        public ReaderParameters ReaderParameters {
            get => readerParameters_ ?? new ReaderParameters();
            set => readerParameters_ = value;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (cache.TryGetValue(name.FullName, out AssemblyDefinition assembly)) 
                return assembly;
            else 
                return cache[name.FullName] = Resolve(name, ReaderParameters);
        }

        protected void RegisterAssembly(AssemblyDefinition assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");
            string name = assembly.Name.FullName;
            if (!cache.ContainsKey(name))
                cache[name] = assembly;
        }

        protected override void Dispose(bool disposing) {
            foreach (AssemblyDefinition assemblyDefinition in cache.Values)
                assemblyDefinition.Dispose();
            cache.Clear();
            base.Dispose(disposing);
        }
    }
}
