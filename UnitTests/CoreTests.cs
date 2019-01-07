using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QueryByText;

namespace UnitTests
{
    // TODO: add tests for various number types
    // TODO: add a test for a date tipe

    [TestFixture]
    public class Tests
    {
        private IQueryable<Order> Orders { get; } = new List<Order>
        {
            new Order {Id = 1, ProductType = "Zinc High Grade", RequestedWeight = 1100},
            new Order {Id = 2, ProductType = "Zinc LME", RequestedWeight = 1000}
        }.AsQueryable();

        private FilterExpressionBuilder<Order> Builder { get; } = new FilterExpressionBuilder<Order>();

        [Test]
        public void Equal()
        {
            const string filterStr = "productType eq 'Zinc High Grade'";
            var expr = Builder.TranslateToExpression(filterStr);

            var filtered = Orders.Where(expr).ToList();

            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered.First, Is.EqualTo(Orders.First()));
        }

        [Test]
        public void NotEqual()
        {
            const string filterStr = "productType ne 'Zinc High Grade'";
            var expr = Builder.TranslateToExpression(filterStr);

            var filtered = Orders.Where(expr).ToList();

            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered.First, Is.EqualTo(Orders.ToList()[1]));
        }

        [Test]
        public void GreaterThan()
        {
            const string filterStr = "requestedWeight gt 1000";
            var expr = Builder.TranslateToExpression(filterStr);

            var filtered = Orders.Where(expr).ToList();

            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered.First, Is.EqualTo(Orders.First()));
        }

        [Test]
        public void GreaterThanOrEqual()
        {
            const string filterStr = "requestedWeight ge 1000";
            var expr = Builder.TranslateToExpression(filterStr);

            var filtered = Orders.Where(expr).ToList();

            Assert.That(filtered.Count, Is.EqualTo(2));
        }

        [Test]
        public void LessThan()
        {
            const string filterStr = "requestedWeight lt 1100";
            var expr = Builder.TranslateToExpression(filterStr);

            var filtered = Orders.Where(expr).ToList();

            Assert.That(filtered.Count, Is.EqualTo(1));
            Assert.That(filtered.First, Is.EqualTo(Orders.ToList()[1]));
        }

        [Test]
        public void LessThanOrEqual()
        {
            const string filterStr = "requestedWeight le 1100";
            var expr = Builder.TranslateToExpression(filterStr);

            var filtered = Orders.Where(expr).ToList();

            Assert.That(filtered.Count, Is.EqualTo(2));
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string ProductType { get; set; }
        public double RequestedWeight { get; set; }
    }
}
