// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.ServiceModel;

namespace System.IdentityModel.Tokens
{
    public class SecurityKeyIdentifier : IEnumerable<SecurityKeyIdentifierClause>
    {
        private const int InitialSize = 2;
        private readonly List<SecurityKeyIdentifierClause> _clauses;
        private bool _isReadOnly;

        public SecurityKeyIdentifier()
        {
            _clauses = new List<SecurityKeyIdentifierClause>(InitialSize);
        }

        public SecurityKeyIdentifier(params SecurityKeyIdentifierClause[] clauses)
        {
            if (clauses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clauses");
            }
            _clauses = new List<SecurityKeyIdentifierClause>(clauses.Length);
            for (int i = 0; i < clauses.Length; i++)
            {
                Add(clauses[i]);
            }
        }

        public SecurityKeyIdentifierClause this[int index]
        {
            get { return _clauses[index]; }
        }

        public bool CanCreateKey
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].CanCreateKey)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int Count
        {
            get { return _clauses.Count; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public void Add(SecurityKeyIdentifierClause clause)
        {
            if (_isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.ObjectIsReadOnly));
            }
            if (clause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("clause"));
            }
            _clauses.Add(clause);
        }

        public SecurityKey CreateKey()
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].CanCreateKey)
                {
                    return this[i].CreateKey();
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.KeyIdentifierCannotCreateKey));
        }

        public TClause Find<TClause>() where TClause : SecurityKeyIdentifierClause
        {
            TClause clause;
            if (!TryFind<TClause>(out clause))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.Format(SR.NoKeyIdentifierClauseFound, typeof(TClause)), "TClause"));
            }
            return clause;
        }

        public IEnumerator<SecurityKeyIdentifierClause> GetEnumerator()
        {
            return _clauses.GetEnumerator();
        }

        public void MakeReadOnly()
        {
            _isReadOnly = true;
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.WriteLine("SecurityKeyIdentifier");
                writer.WriteLine("    (");
                writer.WriteLine("    IsReadOnly = {0},", this.IsReadOnly);
                writer.WriteLine("    Count = {0}{1}", this.Count, this.Count > 0 ? "," : "");
                for (int i = 0; i < this.Count; i++)
                {
                    writer.WriteLine("    Clause[{0}] = {1}{2}", i, this[i], i < this.Count - 1 ? "," : "");
                }
                writer.WriteLine("    )");
                return writer.ToString();
            }
        }

        public bool TryFind<TClause>(out TClause clause) where TClause : SecurityKeyIdentifierClause
        {
            for (int i = 0; i < _clauses.Count; i++)
            {
                TClause c = _clauses[i] as TClause;
                if (c != null)
                {
                    clause = c;
                    return true;
                }
            }
            clause = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

