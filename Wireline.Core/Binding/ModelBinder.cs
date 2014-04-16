using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Wireline.Core.Attributes;
using System.Globalization;

namespace Wireline.Core.Binding {
    public class ModelBinder {
        List<PropertyBinding> bindings = new List<PropertyBinding>();
        MethodInfo convert;
        CultureInfo culture = null;
        IFormatProvider provider = null;

        private IFormatProvider GetProvider(Type fType) {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (fType == typeof(String)) {
                return culture.NumberFormat;
            }
            if (fType == typeof(DateTime) || fType == typeof(DateTimeOffset)) {
                return culture.DateTimeFormat;
            }
            return culture.NumberFormat;
        }

        public ModelBinder(Type tgt) {
            culture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            convert = typeof(ModelBinder).GetMethod("Convert");    
             PropertyInfo[] piarray = tgt.GetProperties();

            var hdrProps = piarray.Where(x => Attribute.IsDefined(x, typeof(HttpHeaderBindingAttribute)));            
            foreach (PropertyInfo pi in hdrProps) {
                provider = GetProvider(pi.PropertyType);
                PropertyBinding pb = new PropertyBinding();
                pb.BindType = BindingTypes.HEADER;
                pb.Target = pi;
                foreach (System.Attribute attr in pi.GetCustomAttributes()) {
                    if (attr is HttpHeaderBindingAttribute) {
                        pb.Source = ((HttpHeaderBindingAttribute)attr).Name;
                        pb.IsCaseSensitive = ((HttpHeaderBindingAttribute)attr).IsCaseSensitive;
                    } else if (attr is OptionalBindingAttribute) {
                        pb.IsRequired = false;
                    } else if (attr is RequiredBindingAttribute) {
                        pb.IsRequired = true;
                    } else if (attr is BindAsJsonAttribute) {
                        pb.AsJson = true;
                    } else if (attr is IgnoreTypeMismatchFailuresAttribute) {
                        pb.IgnoreTypeConversionFailures = true;
                    }
                }
                bindings.Add(pb);
            }
            var ckeProps = piarray.Where(x => Attribute.IsDefined(x, typeof(CookiePropertyBindingAttribute)));
            foreach (PropertyInfo pi in ckeProps) {
                provider = GetProvider(pi.PropertyType);
                PropertyBinding pb = new PropertyBinding();
                pb.BindType = BindingTypes.COOKIE;
                pb.Target = pi;
                foreach (System.Attribute attr in pi.GetCustomAttributes()) {
                    if (attr is CookiePropertyBindingAttribute) {
                        pb.Source = ((CookiePropertyBindingAttribute)attr).Name;
                        pb.IsCaseSensitive = true;
                    } else if (attr is OptionalBindingAttribute) {
                        pb.IsRequired = false;
                    } else if (attr is RequiredBindingAttribute) {
                        pb.IsRequired = true;
                    } else if (attr is BindAsJsonAttribute) {
                        pb.AsJson = true;
                    } else if (attr is IgnoreTypeMismatchFailuresAttribute) {
                        pb.IgnoreTypeConversionFailures = true;
                    }
                }
                bindings.Add(pb);
            }
            var frmProps = piarray.Where(x => Attribute.IsDefined(x, typeof(FormPropertyBindingAttribute)));
            foreach (PropertyInfo pi in frmProps) {
                provider = GetProvider(pi.PropertyType);
                PropertyBinding pb = new PropertyBinding();
                pb.BindType = BindingTypes.FORM;
                pb.Target = pi;
                foreach (System.Attribute attr in pi.GetCustomAttributes()) {
                    if (attr is FormPropertyBindingAttribute) {
                        pb.Source = ((FormPropertyBindingAttribute)attr).Name;
                        pb.IsCaseSensitive = ((FormPropertyBindingAttribute)attr).IsCaseSensitive;
                    } else if (attr is OptionalBindingAttribute) {
                        pb.IsRequired = false;
                    } else if (attr is RequiredBindingAttribute) {
                        pb.IsRequired = true;
                    } else if (attr is BindAsJsonAttribute) {
                        pb.AsJson = true;
                    } else if (attr is IgnoreTypeMismatchFailuresAttribute) {
                        pb.IgnoreTypeConversionFailures = true;
                    }
                }
                bindings.Add(pb);
            }
            var rteProps = piarray.Where(x => Attribute.IsDefined(x, typeof(RouteMatchBindingAttribute)));
            foreach (PropertyInfo pi in rteProps) {
                provider = GetProvider(pi.PropertyType);
                PropertyBinding pb = new PropertyBinding();
                pb.BindType = BindingTypes.ROUTE;
                pb.Target = pi;
                foreach (System.Attribute attr in pi.GetCustomAttributes()) {
                    if (attr is RouteMatchBindingAttribute) {
                        pb.Source = ((RouteMatchBindingAttribute)attr).Name;
                        pb.IsCaseSensitive = true;
                    } else if (attr is OptionalBindingAttribute) {
                        pb.IsRequired = false;
                    } else if (attr is RequiredBindingAttribute) {
                        pb.IsRequired = true;
                    } else if (attr is BindAsJsonAttribute) {
                        pb.AsJson = true;
                    } else if (attr is IgnoreTypeMismatchFailuresAttribute) {
                        pb.IgnoreTypeConversionFailures = true;
                    }
                }
                bindings.Add(pb);
            }
        }

        public void Bind(HttpRequest source, ref Object target) {
            foreach (PropertyBinding pb in bindings) {
                try {
                    if (pb.BindType == BindingTypes.COOKIE) {
                        if (source.Request.Cookies[pb.Source] != null) {
                            String tgt = source.Request.Cookies[pb.Source].Value;
                            if (tgt != null) {
                                var val = Convert(pb.Target.PropertyType, tgt, provider);
                                pb.Target.SetValue(target, val);
                            }
                        }
                    } else if (pb.BindType == BindingTypes.FORM) {
                        // Look for querystring
                        if (source.QueryCollection != null) {
                            String[] queryKeys = null;
                            if (pb.IsCaseSensitive) {
                                queryKeys = source.QueryCollection.AllKeys.Where(x => x == pb.Source).ToArray();
                            } else {
                                queryKeys = source.QueryCollection.AllKeys.Where(x => x.ToUpperInvariant() == pb.Source.ToUpperInvariant()).ToArray();
                            }

                            if (queryKeys != null && queryKeys.Length > 0) {
                                foreach (String key in queryKeys) {
                                    var val = Convert(pb.Target.PropertyType,  source.QueryCollection[key], provider );
                                    pb.Target.SetValue(target, val);
                                }
                            }
                        }
                        // Look for post
                        if (source.FormCollection != null) {
                            String[] formKeys = null;
                            if (pb.IsCaseSensitive) {
                                formKeys = source.FormCollection.AllKeys.Where(x => x == pb.Source).ToArray();
                            } else {
                                formKeys = source.FormCollection.AllKeys.Where(x => x.ToUpperInvariant() == pb.Source.ToUpperInvariant()).ToArray();
                            }
                            if (formKeys != null && formKeys.Length > 0) {
                                foreach (String key in formKeys) {
                                    var val = Convert(pb.Target.PropertyType, source.FormCollection[key], provider);
                                    pb.Target.SetValue(target, val);
                                }
                            }
                        }
                        // look for attachment
                        if (source.AttachmentList != null && source.AttachmentList.Count > 0) {
                            HttpAttachment[] attachs = null;
                            if (pb.IsCaseSensitive) {
                                attachs = source.AttachmentList.Where(x => x.Name == pb.Source).ToArray();
                            } else {
                                attachs = source.AttachmentList.Where(x => x.Name.ToUpperInvariant() == pb.Source.ToUpperInvariant()).ToArray();
                            }
                            if (attachs != null && attachs.Length > 0) {
                                foreach (HttpAttachment attach in attachs) {
                                    var val = Convert(pb.Target.PropertyType, attach.Body, provider);
                                    pb.Target.SetValue(target, val);
                                }
                            }
                        }
                    } else if (pb.BindType == BindingTypes.HEADER) {
                        String[] headerKeys = null;
                        if (pb.IsCaseSensitive) {
                            headerKeys = source.HeaderCollection.AllKeys.Where(x => x == pb.Source).ToArray();
                        } else {
                            headerKeys = source.HeaderCollection.AllKeys.Where(x => x.ToUpperInvariant() == pb.Source.ToUpperInvariant()).ToArray();
                        }
                        if (headerKeys != null && headerKeys.Length > 0) {
                            foreach (String key in headerKeys) {
                                var val = Convert(pb.Target.PropertyType,source.HeaderCollection[key], provider);
                                pb.Target.SetValue(target, val);
                            }
                        }
                    } else if (pb.BindType == BindingTypes.ROUTE) {
                        // Routes are always case-sensitive
                        String[] routeKeys = null;
                        routeKeys = source.RouteCollection.AllKeys.Where(x => x == pb.Source).ToArray();
                        if (routeKeys != null && routeKeys.Length > 0) {
                            foreach (String key in routeKeys) {
                                var val = Convert(pb.Target.PropertyType, source.RouteCollection[key], provider);
                                pb.Target.SetValue(target, val);
                            }
                        }
                    }
                } catch (TypeMismatchBindingException ex) {
                    if (pb.IgnoreTypeConversionFailures == false) {
                        throw ex;
                    }
                } catch (Exception e) {
                    throw e;
                }
            }
        }

        private object Convert(Type pType, String val, IFormatProvider provider) {
            if ((pType == typeof(Boolean) || pType == typeof(bool)) && val.ToUpperInvariant() == "ON") {
                return true;
            }
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(pType);
            return converter.ConvertFromString(val);
        }

        private object Convert(Type pType, byte[] val, IFormatProvider provider) {
            return Convert(pType, System.Text.Encoding.UTF8.GetString(val), provider);
        }

        private MethodInfo GetConverter(Type t) {
            MethodInfo converter = convert.MakeGenericMethod(t);

            return converter;
        }
        
        /// <summary>
        /// Generic wrapper around String's IConvertible implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        T Convert<T>(String value, IFormatProvider provider) where T : Type {
            try {
                
                return (T)((IConvertible)value).ToType(typeof(T), provider);
            } catch (Exception e) {
                throw new TypeMismatchBindingException("Could not convert '" + value + "' to type " + typeof(T).ToString(), e);
            }
        }
    }
}
