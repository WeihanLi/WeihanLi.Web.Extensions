using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Linq;

namespace WeihanLi.Web.DataProtection.ParamsProtection
{
    internal static class ParamsProtectionHelper
    {
        public const string DefaultPurpose = "ParamsProtector";

        private static bool IsParamNeedProtect(this ParamsProtectionOptions option, string propName, string value)
        {
            if (option.ProtectParams.Any(p => p.Equals(propName, StringComparison.OrdinalIgnoreCase)))
            {
                return (!option.ParamValueProtectFuncEnabled || option.ParamValueNeedProtectFunc(value));
            }
            return false;
        }

        private static bool IsParamNeedUnprotect(this ParamsProtectionOptions option, string propName, string value)
        {
            if (option.ProtectParams.Any(p => p.Equals(propName, StringComparison.OrdinalIgnoreCase)))
            {
                return !option.ParamValueProtectFuncEnabled || !option.ParamValueNeedProtectFunc(value);
            }
            return false;
        }

        private static bool IsParamValueNeedProtect(this ParamsProtectionOptions option, string value)
        {
            return !option.ParamValueProtectFuncEnabled || option.ParamValueNeedProtectFunc(value);
        }

        private static void ProtectParams(JToken token, ITimeLimitedDataProtector protector, ParamsProtectionOptions option)
        {
            if (token is JArray array)
            {
                foreach (var j in array)
                {
                    if (array.Parent is JProperty property && j is JValue val)
                    {
                        var strJ = val.Value.ToString();
                        if (option.IsParamNeedProtect(property.Name, strJ))
                        {
                            val.Value = protector.Protect(strJ, TimeSpan.FromMinutes(option.ExpiresIn.GetValueOrDefault(10)));
                        }
                    }
                    else
                    {
                        ProtectParams(j, protector, option);
                    }
                }
            }
            else if (token is JObject obj)
            {
                foreach (var property in obj.Children<JProperty>())
                {
                    var val = property.Value.ToString();
                    if (option.IsParamNeedProtect(property.Name, val))
                    {
                        property.Value = protector.Protect(val, TimeSpan.FromMinutes(option.ExpiresIn.GetValueOrDefault(10)));
                    }
                    else
                    {
                        if (property.Value.HasValues)
                        {
                            ProtectParams(property.Value, protector, option);
                        }
                    }
                }
            }
        }

        public static bool TryGetUnprotectedValue(this IDataProtector protector, ParamsProtectionOptions option,
            string value, out string unprotectedValue)
        {
            if (option.AllowUnprotectedParams &&
                (option.ParamValueProtectFuncEnabled && option.ParamValueNeedProtectFunc(value))
                )
            {
                unprotectedValue = value;
            }
            else
            {
                try
                {
                    unprotectedValue = protector.Unprotect(value);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e, $"Error in unprotect value:{value}");
                    unprotectedValue = value;
                    if (option.AllowUnprotectedParams && e is CryptographicException && !e.Message.Contains("expired"))
                    {
                        return true;
                    }                    
                    return false;
                }
            }
            return true;
        }

        public static void ProtectParams(JToken token, IDataProtector protector, ParamsProtectionOptions option)
        {
            if (option.Enabled && option.ProtectParams?.Length > 0)
            {
                if (protector is ITimeLimitedDataProtector timeLimitedDataProtector)
                {
                    ProtectParams(token, timeLimitedDataProtector, option);
                    return;
                }
                if (token is JArray array)
                {
                    foreach (var j in array)
                    {
                        if (array.Parent is JProperty property && j is JValue val)
                        {
                            var strJ = val.Value.ToString();
                            if (option.IsParamNeedProtect(property.Name, strJ))
                            {
                                val.Value = protector.Protect(strJ);
                            }
                        }
                        else
                        {
                            ProtectParams(j, protector, option);
                        }
                    }
                }
                else if (token is JObject obj)
                {
                    foreach (var property in obj.Children<JProperty>())
                    {
                        var val = property.Value.ToString();
                        if (option.IsParamNeedProtect(property.Name, val))
                        {
                            property.Value = protector.Protect(val);
                        }
                        else
                        {
                            if (property.Value.HasValues)
                            {
                                ProtectParams(property.Value, protector, option);
                            }
                        }
                    }
                }
            }
        }

        public static void UnprotectParams(JToken token, IDataProtector protector, ParamsProtectionOptions option)
        {
            if (option.Enabled && option.ProtectParams?.Length > 0)
            {
                if (token is JArray array)
                {
                    foreach (var j in array)
                    {
                        if (j is JValue val)
                        {
                            var strJ = val.Value.ToString();
                            if (array.Parent is JProperty property && option.IsParamNeedUnprotect(property.Name, strJ))
                            {
                                try
                                {
                                    val.Value = protector.Unprotect(strJ);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                    if (option.AllowUnprotectedParams && e is CryptographicException && !e.Message.Contains("expired"))
                                    {
                                        val.Value = strJ;
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                        else
                        {
                            UnprotectParams(j, protector, option);
                        }
                    }
                }
                else if (token is JObject obj)
                {
                    foreach (var property in obj.Children<JProperty>())
                    {
                        if (property.Value is JArray)
                        {
                            UnprotectParams(property.Value, protector, option);
                        }
                        else
                        {
                            var val = property.Value.ToString();
                            if (option.IsParamNeedUnprotect(property.Name, val))
                            {
                                try
                                {
                                    property.Value = protector.Unprotect(val);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                    if (option.AllowUnprotectedParams && e is CryptographicException && !e.Message.Contains("expired"))
                                    {
                                        property.Value = val;
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                            }
                            else
                            {
                                if (property.Value.HasValues)
                                {
                                    UnprotectParams(property.Value, protector, option);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
