﻿using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.Json.Diagnostics.Internal;

public class JsonLoggingDefinitions : RelationalLoggingDefinitions
{
    public EventDefinitionBase? LogDefaultDecimalTypeColumn;

    public EventDefinitionBase? LogByteIdentityColumn;

    public EventDefinitionBase? LogFoundDefaultSchema;

    public EventDefinitionBase? LogFoundTypeAlias;

    public EventDefinitionBase? LogFoundColumn;

    public EventDefinitionBase? LogFoundForeignKey;

    public EventDefinitionBase? LogPrincipalTableNotInSelectionSet;

    public EventDefinitionBase? LogMissingSchema;

    public EventDefinitionBase? LogMissingTable;

    public EventDefinitionBase? LogFoundSequence;

    public EventDefinitionBase? LogFoundTable;

    public EventDefinitionBase? LogFoundIndex;

    public EventDefinitionBase? LogFoundPrimaryKey;

    public EventDefinitionBase? LogFoundUniqueConstraint;

    public EventDefinitionBase? LogPrincipalColumnNotFound;

    public EventDefinitionBase? LogReflexiveConstraintIgnored;

    public EventDefinitionBase? LogDefaultValueNotSupported;

}
