﻿using FluentAssertions;
using NUnit.Framework;
using Papyrus.Business.Exporters;

namespace Papyrus.Tests.Business {
    [TestFixture]
    public class PathByProductGeneratorShould {
        // TODO
        //   gets the structure /Version/Product/language

        [Test]
        public void gets_product_path() {
            var generator = new PathByProductGenerator();
            
            var path = generator.ForLanguage("es-ES")
                .ForProduct("Opportunity")
                .ForVersion("15.11.20")
                .GeneratePath();

            path.Should().Be("15.11.20/Opportunity/es-ES");
        }
    }
}