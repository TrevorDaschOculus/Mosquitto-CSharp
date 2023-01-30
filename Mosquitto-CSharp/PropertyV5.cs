namespace Mosquitto { 
    public enum PropertyV5
    {
        PayloadFormatIndicator = Native.mqtt5_property.MQTT_PROP_PAYLOAD_FORMAT_INDICATOR,          /* Byte :               PUBLISH, Will Properties */
        MessageExpiryInterval = Native.mqtt5_property.MQTT_PROP_MESSAGE_EXPIRY_INTERVAL,            /* 4 byte int :         PUBLISH, Will Properties */
        ContentType = Native.mqtt5_property.MQTT_PROP_CONTENT_TYPE,                                 /* UTF-8 string :       PUBLISH, Will Properties */
        ResponseTopic = Native.mqtt5_property.MQTT_PROP_RESPONSE_TOPIC,                             /* UTF-8 string :       PUBLISH, Will Properties */
        CorrelationData = Native.mqtt5_property.MQTT_PROP_CORRELATION_DATA,                         /* Binary Data :        PUBLISH, Will Properties */
        SubscriptionIdentifier = Native.mqtt5_property.MQTT_PROP_SUBSCRIPTION_IDENTIFIER,           /* Variable byte int :  PUBLISH, SUBSCRIBE */
        SessionExpiryInterval = Native.mqtt5_property.MQTT_PROP_SESSION_EXPIRY_INTERVAL,            /* 4 byte int :         CONNECT, CONNACK, DISCONNECT */
        AssignedClientIdentifier = Native.mqtt5_property.MQTT_PROP_ASSIGNED_CLIENT_IDENTIFIER,      /* UTF-8 string :       CONNACK */
        ServerKeepAlive = Native.mqtt5_property.MQTT_PROP_SERVER_KEEP_ALIVE,                        /* 2 byte int :         CONNACK */
        AuthenticationMethod = Native.mqtt5_property.MQTT_PROP_AUTHENTICATION_METHOD,               /* UTF-8 string :       CONNECT, CONNACK, AUTH */
        AuthenticationData = Native.mqtt5_property.MQTT_PROP_AUTHENTICATION_DATA,                   /* Binary Data :        CONNECT, CONNACK, AUTH */
        RequestProblemInformation = Native.mqtt5_property.MQTT_PROP_REQUEST_PROBLEM_INFORMATION,    /* Byte :               CONNECT */
        WillDelayInterval = Native.mqtt5_property.MQTT_PROP_WILL_DELAY_INTERVAL,                    /* 4 byte int :         Will properties */
        RequestResponseInformation = Native.mqtt5_property.MQTT_PROP_REQUEST_RESPONSE_INFORMATION,  /* Byte :               CONNECT */
        ResponseInformation = Native.mqtt5_property.MQTT_PROP_RESPONSE_INFORMATION,                 /* UTF-8 string :       CONNACK */
        ServerReference = Native.mqtt5_property.MQTT_PROP_SERVER_REFERENCE,                         /* UTF-8 string :       CONNACK, DISCONNECT */
        ReasonString = Native.mqtt5_property.MQTT_PROP_REASON_STRING,                               /* UTF-8 string :       All except Will properties */
        ReceiveMaximum = Native.mqtt5_property.MQTT_PROP_RECEIVE_MAXIMUM,                           /* 2 byte int :         CONNECT, CONNACK */
        TopicAliasMaximum = Native.mqtt5_property.MQTT_PROP_TOPIC_ALIAS_MAXIMUM,                    /* 2 byte int :         CONNECT, CONNACK */
        TopicAlias = Native.mqtt5_property.MQTT_PROP_TOPIC_ALIAS,                                   /* 2 byte int :         PUBLISH */
        MaximumQos = Native.mqtt5_property.MQTT_PROP_MAXIMUM_QOS,                                   /* Byte :               CONNACK */
        RetainAvailable = Native.mqtt5_property.MQTT_PROP_RETAIN_AVAILABLE,                         /* Byte :               CONNACK */
        UserProperty = Native.mqtt5_property.MQTT_PROP_USER_PROPERTY,                               /* UTF-8 string pair :  All */
        MaximumPacketSize = Native.mqtt5_property.MQTT_PROP_MAXIMUM_PACKET_SIZE,                    /* 4 byte int :         CONNECT, CONNACK */
        WildcardSubAvailable = Native.mqtt5_property.MQTT_PROP_WILDCARD_SUB_AVAILABLE,              /* Byte :               CONNACK */
        SubscriptionIdAvailable = Native.mqtt5_property.MQTT_PROP_SUBSCRIPTION_ID_AVAILABLE,        /* Byte :               CONNACK */
        SharedSubAvailable = Native.mqtt5_property.MQTT_PROP_SHARED_SUB_AVAILABLE,                  /* Byte :               CONNACK */
    }
}