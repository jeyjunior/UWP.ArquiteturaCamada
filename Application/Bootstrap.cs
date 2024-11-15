using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraData;

namespace Application
{
    public class Bootstrap
    {
        public static Container Container { get; private set; }
        public static void Iniciar()
        {
            Container = new Container();
            Container.Options.DefaultLifestyle = Lifestyle.Scoped;

            BootstrapInfraData.Iniciar(Container);

            Container.Verify();

            InfraConfig.Iniciar();
        }
    }
}
