using System;
using System.Text;
using InRule.Authoring.ComponentModel;
using InRule.Runtime;

namespace InRule.RuleApplicationFramework
{
    public class RuleExecutionInfo : ObservableObject
    {
        private TimeSpan _catalogRefreshInterval = new TimeSpan(1, 0, 0);
        private TimeSpan _catalogRuleAppTimeoutInterval = new TimeSpan(0, 1, 0);
        private string _catalogUri;
        private string _ruleEngineUri;
        private string _username;
        private string _password;
        private string _ruleAppFilePath;
        private string _stateFilePath;
        private string _ruleAppName;
        private string _ruleSetName;
        private string _entityName;
        private string _categoryName;
        private bool _useRuleEngineService = false;
        private bool _useCatalogService = false;
        private bool _preserveRuleSession = false;
        private bool _useInMemoryRuleApp = false;
        private bool _compileDelegates = true;
        private bool _preCompileBeforeApplyRules = false;
        private object _stateObject;
        private bool _useObjectForState = false;
        private bool _useVerboseLogging = false;

        private int _maxDegreeOfParallelism = 1;
        
        //private BoundStateSourceMode _boundStateMode = BoundStateSourceMode.Default;
        //private BoundStateCachingMode _boundStateCacheMode = BoundStateCachingMode.None;
        private EngineLogOptions _executionLogOptions = EngineLogOptions.None;
        //private CompilerMode _compileMode = CompilerMode.FullyCompiled;
        private CacheRetention _cacheRetention = CacheRetention.Default;
        
        public RuleExecutionInfo()
        {
        }

        public RuleExecutionInfo(string ruleAppFilePath)
        {
            _ruleAppFilePath = ruleAppFilePath;
        }

        public RuleExecutionInfo(string catalogUri, string username, string password)
        {
            _catalogUri = catalogUri;
            _username = username;
            _password = password;
        }

        public RuleExecutionInfo(string ruleEngineUri, string catalogUri, string username, string password)
        {
            _ruleEngineUri = ruleEngineUri;
            _catalogUri = catalogUri;
            _username = username;
            _password = password;
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string RuleEngineUri
        {
            get { return _ruleEngineUri; }
            set
            {
                _ruleEngineUri = value;
                if (!string.IsNullOrEmpty(_ruleEngineUri))
                    UseRuleEngineService = true;
                else
                    UseRuleEngineService = false;
            }
        }

        public TimeSpan CatalogRefreshInterval
        {
            get { return _catalogRefreshInterval; }
            set { _catalogRefreshInterval = value; }
        }

        public TimeSpan CatalogRuleAppTimeoutInterval
        {
            get { return _catalogRuleAppTimeoutInterval; }
            set { _catalogRuleAppTimeoutInterval = value; }
        }

        public string CatalogUri
        {
            get { return _catalogUri; }
            set
            {
                _catalogUri = value;
                //if (!string.IsNullOrEmpty(_catalogUri))
                //    UseCatalogService = true;
                //else
                //    UseCatalogService = false;
            }
        }

        public string RuleAppFilePath
        {
            get
            {
                return _ruleAppFilePath;
            }
            set
            {
                _ruleAppFilePath = value;
            }
        }
        public string StateFilePath
        {
            get
            {
                return _stateFilePath;
            }
            set
            {
                _stateFilePath = value;

                OnPropertyChanged("StateFilePath");
            }
        }
        public string RuleAppName
        {
            get
            {
                return _ruleAppName;
            }
            set
            {
                _ruleAppName = value;
            }
        }
        public string RuleSetName
        {
            get
            {
                return _ruleSetName;
            }
            set
            {
                _ruleSetName = value;
            }
        }
        public string EntityName
        {
            get
            {
                return _entityName;
            }
            set
            {
                _entityName = value;
            }
        }
        public string CategoryName
        {
            get
            {
                return _categoryName;
            }
            set
            {
                _categoryName = value;
            }
        }

        public bool UseCatalogService
        {
            get
            {
                return _useCatalogService;
            }
            set
            {
                _useCatalogService = value;
            }
        }

        public bool UseRuleEngineService
        {
            get
            {
                return _useRuleEngineService;
            }
            set
            {
                _useRuleEngineService = value;
            }
        }

        public bool PreserveRuleSession
        {
            get
            {
                return _preserveRuleSession;
            }
            set
            {
                _preserveRuleSession = value;
            }
        }

        public bool PreCompileBeforeApplyRules
        {
            get { return _preCompileBeforeApplyRules; }
            set { _preCompileBeforeApplyRules = value; }
        }

        public bool CompileDelegates
        {
            get
            {
                return _compileDelegates;
            }
            set
            {
                _compileDelegates = value;
            }
        }

        public int MaxDegreeOfParallelism
        {
            get
            {
                return _maxDegreeOfParallelism;
            }
            set
            {
                _maxDegreeOfParallelism = value;
            }
        }

        //public BoundStateSourceMode BoundStateMode
        //{
        //    get { return _boundStateMode; }
        //    set { _boundStateMode = value; }
        //}

        public EngineLogOptions ExecutionLogOptions
        {
            get { return _executionLogOptions; }
            set { _executionLogOptions = value; }
        }

        //public BoundStateCachingMode BoundStateCacheMode
        //{
        //    get { return _boundStateCacheMode; }
        //    set { _boundStateCacheMode = value; }
        //}

        public bool UseObjectForState
        {
            get { return _useObjectForState; }
            set { _useObjectForState = value; }
        }

        public bool UseVerboseLogging
        {
            get { return _useVerboseLogging; }
            set { _useVerboseLogging = value; }
        }

        public object StateObject
        {
            get { return _stateObject; }
            set { _stateObject = value; }
        }
        public bool UseInMemoryRuleApp
        {
            get { return _useInMemoryRuleApp; }
            set { _useInMemoryRuleApp = value; }
        }
        /*
        public CompilerMode CompileMode
        {
            get { return _compileMode; }
            set { _compileMode = value; }
        }
        */
        public CacheRetention CacheRetention
        {
            get { return _cacheRetention; }
            set { _cacheRetention = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--> Services Config <--" + Environment.NewLine);

            if (UseCatalogService)
            {
                sb.Append("Repository URI: " + CatalogUri + Environment.NewLine);
                sb.Append("Username: " + Username + Environment.NewLine);
                sb.Append("Password: " + Password + Environment.NewLine);
            }
            else
            {
                sb.Append("RuleAppFilePath: " + RuleAppFilePath + Environment.NewLine);
            }

            if (UseRuleEngineService)
            {
                sb.Append("RuleEngine URI: " + RuleEngineUri + Environment.NewLine);
            }
            else
            {
                sb.Append("RuleEngine: InProcess" + Environment.NewLine);
            }

            return sb.ToString();
        }

        public void Clear()
        {
            RuleAppName = "";
            //RuleAppReference = null;
            CategoryName = "";
            EntityName = "";
            RuleAppFilePath = "";
            RuleSetName = "";
            StateFilePath = "";

        }
    }
}
