import * as signalR from '@microsoft/signalr';
import { getServiceBaseURL } from '@/utils/service';
import { getToken } from '@/store/modules/auth/shared';

let connection: signalR.HubConnection | null = null;

export function useOnlineHub() {
  async function connect() {
    const token = getToken();
    if (!token) return;

    if (connection?.state === signalR.HubConnectionState.Connected) return;
    if (connection?.state === signalR.HubConnectionState.Connecting) return;

    if (connection) {
      await connection.stop();
      connection = null;
    }

    const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
    const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseURL}/hubs/online`, {
        accessTokenFactory: () => getToken()
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    try {
      await connection.start();
    } catch {
      connection = null;
    }
  }

  async function disconnect() {
    if (!connection) return;
    try {
      await connection.stop();
    } finally {
      connection = null;
    }
  }

  return { connect, disconnect };
}
