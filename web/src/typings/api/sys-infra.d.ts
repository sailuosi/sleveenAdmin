declare namespace Api {
  namespace SysInfra {
    type GuidRecord = { id: string };

    type SysFile = GuidRecord & {
      originalName: string;
      storedName: string;
      extension: string | null;
      size: number;
      contentType: string | null;
      path: string;
      url: string;
      createUserId: string | null;
      createTime: string;
    };

    type SysLogOp = GuidRecord & {
      userId: string | null;
      account: string | null;
      module: string | null;
      action: string | null;
      httpMethod: string | null;
      requestUrl: string | null;
      requestParam: string | null;
      statusCode: number;
      success: boolean;
      elapsedMs: number;
      ip: string | null;
      createTime: string;
    };

    type SysLogVis = GuidRecord & {
      userId: string | null;
      account: string | null;
      actionName: string | null;
      requestUrl: string | null;
      httpMethod: string | null;
      statusCode: number;
      elapsedMs: number;
      ip: string | null;
      createTime: string;
    };

    type FilePageParams = {
      page?: number;
      pageSize?: number;
      keyword?: string | null;
    };

    type LogPageParams = {
      page?: number;
      pageSize?: number;
      keyword?: string | null;
      startTime?: string | null;
      endTime?: string | null;
    };

    type DiskInfo = {
      name: string;
      driveType: string;
      totalBytes: number;
      freeBytes: number;
      usedPercent: number;
    };

    type ServerInfo = {
      machineName: string;
      osDescription: string;
      osArchitecture: string;
      frameworkDescription: string;
      processorCount: number;
      workingSetBytes: number;
      gcHeapBytes: number;
      processUptime: string;
      disks: DiskInfo[];
    };

    type RedisInfo = {
      configured: boolean;
      connected: boolean;
      serverVersion: string | null;
      endPoint: string | null;
      dbSize: number | null;
      error: string | null;
    };

    type RedisKey = {
      key: string;
      type: string;
      ttlSeconds: number | null;
    };

    type RedisKeyValue = RedisKey & {
      value: string | null;
    };

    type RedisKeyPageParams = {
      page?: number;
      pageSize?: number;
      keyword?: string | null;
      pattern?: string | null;
    };
  }
}
