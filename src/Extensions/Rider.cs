﻿using JetBrains.Annotations;
using Serilog;

namespace Espeon {
    //TODO extract this to a different project
    public static class RiderExtensions {
        [SourceTemplate]
        [Macro(Target = "clazz", Expression = "typeName()")]
        public static void slog(this ILogger logger) {
            //$ this._logger = logger.ForContext("SourceContext", nameof(clazz));
        }
        
        [SourceTemplate]
        [Macro(Target = "type", Expression = "fixedTypeName()")]
        public static void @is(this object obj) {
            /*$ if (obj is type) {
                $END$
             }*/
        }
    }
}