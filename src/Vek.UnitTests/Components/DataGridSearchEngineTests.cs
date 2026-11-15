// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AwesomeAssertions;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

[TestFixture]
public class DataGridSearchEngineTests
{
    public record TestSearchModel(
        string? Name = null,
        string? Department = null,
        string? City = null,
        int? Age = null,
        DateTime? HireDate = null,
        string? Secret = null);

    private class TestColumn<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : Column<T>
    {
        private readonly Func<T, object?> _cellContentFunc;

        public TestColumn(Func<T, object?> cellContentFunc, bool allowSearch = true, string? format = null, CultureInfo? culture = null)
        {
            _cellContentFunc = cellContentFunc;
            AllowSearch = allowSearch;
            ContentFormat = format;
            Culture = culture;
        }

        protected internal override string? ContentFormat { get; }

        protected internal override object? CellContent(T item) => _cellContentFunc(item);

        protected internal override object? PropertyFunc(T item) => _cellContentFunc(item);

        public override string? PropertyName => "TestColumn";

        protected internal override void SetProperty(object? item, object? value)
        {
        }
    }

    #region ParseTokens Tests

    [TestCase(null, DataGridSearchTextParseMode.ExactMatch)]
    [TestCase("", DataGridSearchTextParseMode.ExactMatch)]
    [TestCase("   ", DataGridSearchTextParseMode.ExactMatch)]
    [TestCase(null, DataGridSearchTextParseMode.GroupWordsByAnd)]
    [TestCase("", DataGridSearchTextParseMode.GroupWordsByAnd)]
    [TestCase("   \t  \n ", DataGridSearchTextParseMode.GroupWordsByAnd)]
    [TestCase(null, DataGridSearchTextParseMode.GroupWordsByOr)]
    [TestCase("", DataGridSearchTextParseMode.GroupWordsByOr)]
    [TestCase("   ", DataGridSearchTextParseMode.GroupWordsByOr)]
    public void ParseTokens_NullOrWhitespace_ReturnsEmptyList(string? searchText, DataGridSearchTextParseMode parseMode)
    {
        var tokens = DataGridSearchEngine.ParseTokens(searchText, parseMode);

        tokens.Should().BeEmpty();
    }

    [Test]
    public void ParseTokens_ExactMatch_ReturnsSingleTrimmedToken()
    {
        var singleWord = DataGridSearchEngine.ParseTokens("hello", DataGridSearchTextParseMode.ExactMatch);
        singleWord.Should().Equal("hello");

        var phraseWithSpaces = DataGridSearchEngine.ParseTokens("  hello world  ", DataGridSearchTextParseMode.ExactMatch);
        phraseWithSpaces.Should().Equal("hello world");

        var phraseWithMultipleSpaces = DataGridSearchEngine.ParseTokens("hello   world", DataGridSearchTextParseMode.ExactMatch);
        phraseWithMultipleSpaces.Should().Equal("hello   world");

        var withQuotes = DataGridSearchEngine.ParseTokens("\"hello world\"", DataGridSearchTextParseMode.ExactMatch);
        withQuotes.Should().Equal("\"hello world\"");
    }

    [Test]
    public void ParseTokens_GroupWordsByAnd_SplitsByWhitespace()
    {
        var tokens = DataGridSearchEngine.ParseTokens("alpha beta gamma", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("alpha", "beta", "gamma");
    }

    [Test]
    public void ParseTokens_GroupWordsByAnd_CollapsesMultipleSpaces()
    {
        var tokens = DataGridSearchEngine.ParseTokens("   alpha     beta   ", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("alpha", "beta");
    }

    [Test]
    public void ParseTokens_GroupWordsByOr_SplitsByWhitespace()
    {
        var tokens = DataGridSearchEngine.ParseTokens("cat dog bird", DataGridSearchTextParseMode.GroupWordsByOr);

        tokens.Should().Equal("cat", "dog", "bird");
    }

    [Test]
    public void ParseTokens_Quotes_GroupsQuotedPhraseIntoSingleToken()
    {
        var tokens = DataGridSearchEngine.ParseTokens("hello \"world wide web\" test", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("hello", "world wide web", "test");
    }

    [Test]
    public void ParseTokens_Quotes_OnlyQuotedPhrase()
    {
        var tokens = DataGridSearchEngine.ParseTokens("\"phrase with spaces\"", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("phrase with spaces");
    }

    [Test]
    public void ParseTokens_Quotes_MultipleQuotedPhrases()
    {
        var tokens = DataGridSearchEngine.ParseTokens("\"first phrase\" \"second phrase\"", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("first phrase", "second phrase");
    }

    [Test]
    public void ParseTokens_Quotes_QuoteAtStartAndEnd()
    {
        var startTokens = DataGridSearchEngine.ParseTokens("\"quote at start\" word", DataGridSearchTextParseMode.GroupWordsByAnd);
        startTokens.Should().Equal("quote at start", "word");

        var endTokens = DataGridSearchEngine.ParseTokens("word \"quote at end\"", DataGridSearchTextParseMode.GroupWordsByAnd);
        endTokens.Should().Equal("word", "quote at end");
    }

    [Test]
    public void ParseTokens_Quotes_AdjacentQuotesWithoutWhitespace()
    {
        var tokens = DataGridSearchEngine.ParseTokens("\"first\"\"second\"", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("first", "second");
    }

    [Test]
    public void ParseTokens_Quotes_EmptyQuotesIgnored()
    {
        var tokens = DataGridSearchEngine.ParseTokens("\"\" hello \"\"", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("hello");
    }

    [Test]
    public void ParseTokens_Quotes_WhitespaceOnlyQuotesIgnored()
    {
        var tokens = DataGridSearchEngine.ParseTokens("\"   \" hello", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("hello");
    }

    [Test]
    public void ParseTokens_Quotes_UnclosedQuoteTreatedAsTokenToEnd()
    {
        var tokens = DataGridSearchEngine.ParseTokens("foo \"bar baz", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("foo", "bar baz");
    }

    [Test]
    public void ParseTokens_Quotes_EscapedQuotesSupported()
    {
        var tokens = DataGridSearchEngine.ParseTokens("\"say \\\"hello\\\" world\"", DataGridSearchTextParseMode.GroupWordsByAnd);

        tokens.Should().Equal("say \"hello\" world");
    }

    [Test]
    public void ParseTokens_InvalidParseMode_ThrowsArgumentOutOfRangeException()
    {
        var act = () => DataGridSearchEngine.ParseTokens("test", (DataGridSearchTextParseMode)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region MatchesRow Null and Empty Guard Tests

    [Test]
    public void MatchesRow_NullItem_ReturnsFalse()
    {
        var column = new TestColumn<TestSearchModel>(x => x.Name);
        var tokens = new[] { "alice" };

        var matches = DataGridSearchEngine.MatchesRow<TestSearchModel>(
            null!,
            new[] { column },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeFalse();
    }

    [Test]
    public void MatchesRow_NullColumns_ReturnsFalse()
    {
        var item = new TestSearchModel(Name: "Alice");
        var tokens = new[] { "alice" };

        var matches = DataGridSearchEngine.MatchesRow<TestSearchModel>(
            item,
            null!,
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeFalse();
    }

    [TestCase(DataGridSearchTextParseMode.GroupWordsByAnd)]
    [TestCase(DataGridSearchTextParseMode.GroupWordsByOr)]
    [TestCase(DataGridSearchTextParseMode.ExactMatch)]
    public void MatchesRow_NullOrEmptyTokens_ReturnsTrue(DataGridSearchTextParseMode parseMode)
    {
        var item = new TestSearchModel(Name: "Alice");
        var column = new TestColumn<TestSearchModel>(x => x.Name);

        DataGridSearchEngine.MatchesRow(item, new[] { column }, null!, parseMode).Should().BeTrue();
        DataGridSearchEngine.MatchesRow(item, new[] { column }, Array.Empty<string>(), parseMode).Should().BeTrue();
        DataGridSearchEngine.MatchesRow(item, new[] { column }, new[] { "   ", "" }, parseMode).Should().BeTrue();
    }

    [Test]
    public void MatchesRow_NoSearchableColumns_ReturnsFalse()
    {
        var item = new TestSearchModel(Name: "Alice");
        var tokens = new[] { "Alice" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            Array.Empty<Column<TestSearchModel>>(),
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeFalse();
    }

    [Test]
    public void MatchesRow_AllNullCells_ReturnsFalse()
    {
        var item = new TestSearchModel(Name: null, Department: null);
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var tokens = new[] { "Alice" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeFalse();
    }

    #endregion

    #region GroupWordsByAnd Tests

    [Test]
    public void MatchesRow_GroupWordsByAnd_AllTokensMatchSingleColumn_ReturnsTrue()
    {
        var item = new TestSearchModel(Name: "Johnathan Doe");
        var column = new TestColumn<TestSearchModel>(x => x.Name);
        var tokens = new[] { "John", "Doe" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { column },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_GroupWordsByAnd_TokensAcrossMultipleColumns_ReturnsTrue()
    {
        var item = new TestSearchModel(Name: "Alice", Department: "Engineering", City: "Seattle");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var col3 = new TestColumn<TestSearchModel>(x => x.City);
        var tokens = new[] { "Alice", "Engineering", "Seattle" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2, col3 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_GroupWordsByAnd_OneTokenMissing_ReturnsFalse()
    {
        var item = new TestSearchModel(Name: "Alice", Department: "Engineering", City: "Seattle");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var col3 = new TestColumn<TestSearchModel>(x => x.City);
        var tokens = new[] { "Alice", "Engineering", "Chicago" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2, col3 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeFalse();
    }

    [Test]
    public void MatchesRow_GroupWordsByAnd_CaseInsensitive()
    {
        var item = new TestSearchModel(Name: "Alice Smith", Department: "DESIGN");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var tokens = new[] { "alice", "SMITH", "design" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeTrue();
    }

    #endregion

    #region GroupWordsByOr Tests

    [Test]
    public void MatchesRow_GroupWordsByOr_FirstTokenMatches_ReturnsTrue()
    {
        var item = new TestSearchModel(Name: "Alice", Department: "IT");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var tokens = new[] { "Alice", "Marketing" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByOr);

        matches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_GroupWordsByOr_SecondTokenMatches_ReturnsTrue()
    {
        var item = new TestSearchModel(Name: "Alice", Department: "IT");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var tokens = new[] { "Finance", "IT" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByOr);

        matches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_GroupWordsByOr_NoTokensMatch_ReturnsFalse()
    {
        var item = new TestSearchModel(Name: "Alice", Department: "IT");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var tokens = new[] { "Bob", "Finance" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByOr);

        matches.Should().BeFalse();
    }

    [Test]
    public void MatchesRow_GroupWordsByOr_CaseInsensitive()
    {
        var item = new TestSearchModel(Name: "Alice");
        var col = new TestColumn<TestSearchModel>(x => x.Name);
        var tokens = new[] { "BOB", "alice" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col },
            tokens,
            DataGridSearchTextParseMode.GroupWordsByOr);

        matches.Should().BeTrue();
    }

    #endregion

    #region ExactMatch Tests

    [Test]
    public void MatchesRow_ExactMatch_EntirePhraseInSingleColumn_ReturnsTrue()
    {
        var item = new TestSearchModel(Name: "Senior Software Engineer");
        var column = new TestColumn<TestSearchModel>(x => x.Name);
        var tokens = new[] { "Software Engineer" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { column },
            tokens,
            DataGridSearchTextParseMode.ExactMatch);

        matches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_ExactMatch_PhraseSplitAcrossColumns_ReturnsFalse()
    {
        var item = new TestSearchModel(Name: "Software", Department: "Engineer");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department);
        var tokens = new[] { "Software Engineer" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            tokens,
            DataGridSearchTextParseMode.ExactMatch);

        matches.Should().BeFalse();
    }

    [Test]
    public void MatchesRow_ExactMatch_CaseInsensitive()
    {
        var item = new TestSearchModel(Name: "Senior Software Engineer");
        var column = new TestColumn<TestSearchModel>(x => x.Name);
        var tokens = new[] { "senior software engineer" };

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { column },
            tokens,
            DataGridSearchTextParseMode.ExactMatch);

        matches.Should().BeTrue();
    }

    #endregion

    #region AllowSearch and Column Searchability Tests

    [Test]
    public void MatchesRow_ColumnWithAllowSearchFalse_IsExcludedFromSearch()
    {
        var item = new TestSearchModel(Name: "Alice", Secret: "SecretToken123");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name, allowSearch: true);
        var col2 = new TestColumn<TestSearchModel>(x => x.Secret, allowSearch: false);

        // Searching for secret should NOT match because AllowSearch is false on col2
        var secretMatches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "SecretToken123" },
            DataGridSearchTextParseMode.GroupWordsByAnd);
        secretMatches.Should().BeFalse();

        // Searching for name should match because AllowSearch is true on col1
        var nameMatches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "Alice" },
            DataGridSearchTextParseMode.GroupWordsByAnd);
        nameMatches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_CustomSearchablePredicate_OverridesAllowSearch()
    {
        var item = new TestSearchModel(Name: "Alice", Department: "Finance");
        var col1 = new TestColumn<TestSearchModel>(x => x.Name, allowSearch: true);
        var col2 = new TestColumn<TestSearchModel>(x => x.Department, allowSearch: true);

        // Custom predicate that only permits Department
        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "Alice" },
            DataGridSearchTextParseMode.GroupWordsByAnd,
            culture: null,
            isColumnSearchable: col => col == col2);

        matches.Should().BeFalse();

        var deptMatches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "Finance" },
            DataGridSearchTextParseMode.GroupWordsByAnd,
            culture: null,
            isColumnSearchable: col => col == col2);

        deptMatches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_WithPropertyColumn_WorksDirectly()
    {
        var item = new TestSearchModel(Name: "Charlie Brown", Department: "Peanuts");
#pragma warning disable BL0005
        var col1 = new PropertyColumn<TestSearchModel, string> { Property = x => x.Name! };
        var col2 = new PropertyColumn<TestSearchModel, string> { Property = x => x.Department! };
#pragma warning restore BL0005

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new Column<TestSearchModel>[] { col1, col2 },
            new[] { "Charlie", "Peanuts" },
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeTrue();
    }

    #endregion

    #region Data Types, Formatting and Culture Tests

    [Test]
    public void MatchesRow_NumericColumn_MatchesNumbers()
    {
        var item = new TestSearchModel(Name: "Bob", Age: 42);
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.Age);

        var matches = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "42" },
            DataGridSearchTextParseMode.GroupWordsByAnd);

        matches.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_FormattedDateColumn_MatchesFormattedStringAndRawYear()
    {
        var item = new TestSearchModel(Name: "Bob", HireDate: new DateTime(2023, 11, 15));
        var col1 = new TestColumn<TestSearchModel>(x => x.Name);
        var col2 = new TestColumn<TestSearchModel>(x => x.HireDate, format: "yyyy-MM-dd");

        var matchesFormatted = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "2023-11-15" },
            DataGridSearchTextParseMode.GroupWordsByAnd);
        matchesFormatted.Should().BeTrue();

        var matchesYear = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col1, col2 },
            new[] { "2023" },
            DataGridSearchTextParseMode.GroupWordsByAnd);
        matchesYear.Should().BeTrue();
    }

    [Test]
    public void MatchesRow_CultureSpecific_RespectsCultureForCaseInsensitiveSearch()
    {
        var turkishCulture = new CultureInfo("tr-TR");
        // In Turkish, uppercase of 'i' is 'İ' (\u0130), lowercase of 'I' is 'ı' (\u0131)
        var item = new TestSearchModel(City: "DİYARBAKIR");
        var col = new TestColumn<TestSearchModel>(x => x.City, culture: turkishCulture);

        var matchesTurkish = DataGridSearchEngine.MatchesRow(
            item,
            new[] { col },
            new[] { "diyarbakır" },
            DataGridSearchTextParseMode.GroupWordsByAnd,
            culture: turkishCulture);

        matchesTurkish.Should().BeTrue();
    }

    #endregion

    #region End-to-End Integration Tests

    [Test]
    public void EndToEnd_ParseAndMatch_AndModeWithQuotedPhrases()
    {
        var person = new TestSearchModel(Name: "John Smith", Department: "Chief Technology Officer", City: "New York");
        var columns = new[]
        {
            new TestColumn<TestSearchModel>(x => x.Name),
            new TestColumn<TestSearchModel>(x => x.Department),
            new TestColumn<TestSearchModel>(x => x.City)
        };

        var searchPassing = "John \"Technology Officer\" \"New York\"";
        var tokensPassing = DataGridSearchEngine.ParseTokens(searchPassing, DataGridSearchTextParseMode.GroupWordsByAnd);
        tokensPassing.Should().Equal("John", "Technology Officer", "New York");

        var resultPassing = DataGridSearchEngine.MatchesRow(person, columns, tokensPassing, DataGridSearchTextParseMode.GroupWordsByAnd);
        resultPassing.Should().BeTrue();

        var searchFailing = "John \"Financial Officer\"";
        var tokensFailing = DataGridSearchEngine.ParseTokens(searchFailing, DataGridSearchTextParseMode.GroupWordsByAnd);
        var resultFailing = DataGridSearchEngine.MatchesRow(person, columns, tokensFailing, DataGridSearchTextParseMode.GroupWordsByAnd);
        resultFailing.Should().BeFalse();
    }

    [Test]
    public void EndToEnd_ParseAndMatch_ExactMatchMode()
    {
        var person = new TestSearchModel(Name: "Sarah Connor", Department: "Resistance");
        var columns = new[]
        {
            new TestColumn<TestSearchModel>(x => x.Name),
            new TestColumn<TestSearchModel>(x => x.Department)
        };

        var searchExact = "Sarah Connor";
        var tokensExact = DataGridSearchEngine.ParseTokens(searchExact, DataGridSearchTextParseMode.ExactMatch);
        tokensExact.Should().Equal("Sarah Connor");

        var resultExact = DataGridSearchEngine.MatchesRow(person, columns, tokensExact, DataGridSearchTextParseMode.ExactMatch);
        resultExact.Should().BeTrue();

        var searchReversed = "Connor Sarah";
        var tokensReversed = DataGridSearchEngine.ParseTokens(searchReversed, DataGridSearchTextParseMode.ExactMatch);
        var resultReversed = DataGridSearchEngine.MatchesRow(person, columns, tokensReversed, DataGridSearchTextParseMode.ExactMatch);
        resultReversed.Should().BeFalse();
    }

    [Test]
    public void EndToEnd_ParseAndMatch_OrMode()
    {
        var hero = new TestSearchModel(Name: "Bruce Wayne", Department: "Batman");
        var columns = new[]
        {
            new TestColumn<TestSearchModel>(x => x.Name),
            new TestColumn<TestSearchModel>(x => x.Department)
        };

        var searchOr = "Superman Batman Joker";
        var tokensOr = DataGridSearchEngine.ParseTokens(searchOr, DataGridSearchTextParseMode.GroupWordsByOr);
        tokensOr.Should().Equal("Superman", "Batman", "Joker");

        var resultOr = DataGridSearchEngine.MatchesRow(hero, columns, tokensOr, DataGridSearchTextParseMode.GroupWordsByOr);
        resultOr.Should().BeTrue();

        var searchNone = "Spider-Man Wolverine";
        var tokensNone = DataGridSearchEngine.ParseTokens(searchNone, DataGridSearchTextParseMode.GroupWordsByOr);
        var resultNone = DataGridSearchEngine.MatchesRow(hero, columns, tokensNone, DataGridSearchTextParseMode.GroupWordsByOr);
        resultNone.Should().BeFalse();
    }

    #endregion
}
