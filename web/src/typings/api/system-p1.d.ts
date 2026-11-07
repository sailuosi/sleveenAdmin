declare namespace Api {
  namespace SystemP1 {
    type Tenant = {
      id: string;
      name: string;
      code: string;
      status: number;
      remark: string | null;
      createTime: string;
    };

    type TenantInput = {
      id?: string | null;
      name: string;
      code: string;
      status: number;
      remark?: string | null;
    };

    type Notice = {
      id: string;
      title: string;
      content: string;
      type: number;
      status: number;
      tenantId: string | null;
      createUserId: string;
      createTime: string;
      publishTime: string | null;
      targetUserIds: string[];
    };

    type NoticeInput = {
      id?: string | null;
      title: string;
      content: string;
      type: number;
      targetUserIds?: string[] | null;
    };

    type MyNotice = {
      id: string;
      title: string;
      content: string;
      type: number;
      isRead: boolean;
      publishTime: string | null;
      createTime: string;
    };

    type OnlineUser = {
      connectionId: string;
      userId: string;
      account: string;
      realName: string | null;
      ip: string | null;
      connectedAt: string;
    };
  }
}
