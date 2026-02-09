import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const API = process.env.REACT_APP_API_URL || 'http://localhost:5006';

export function createChatConnection(token) {
  return new HubConnectionBuilder()
    .withUrl(`${API}/hubs/chat`, { accessTokenFactory: () => token })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
