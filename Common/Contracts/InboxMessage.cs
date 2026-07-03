using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts;

public record InboxMessage
(
    DateTimeOffset OccuredOn,

    string MessageType,

    bool Processed,

    int RetryCount,

    string Payload
);

