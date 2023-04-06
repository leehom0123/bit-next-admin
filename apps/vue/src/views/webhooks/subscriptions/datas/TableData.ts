import { useLocalization } from '/@/hooks/abp/useLocalization';
import { BasicColumn } from '/@/components/Table';
import { formatToDateTime } from '/@/utils/dateUtil';

const { L } = useLocalization('WebhooksManagement');

export function getDataColumns(): BasicColumn[] {
  return [
    {
      title: 'id',
      dataIndex: 'id',
      width: 1,
      ifShow: false,
    },
    {
      title: L('DisplayName:TenantId'),
      dataIndex: 'tenantId',
      align: 'left',
      width: 300,
      sorter: true,
      fixed: 'left',
    },
    {
      title: L('DisplayName:WebhookUri'),
      dataIndex: 'webhookUri',
      align: 'left',
      width: 300,
      sorter: true,
      fixed: 'left',
    },
    {
      title: L('DisplayName:IsActive'),
      dataIndex: 'isActive',
      align: 'center',
      width: 180,
      sorter: true,
    },
    {
      title: L('DisplayName:Description'),
      dataIndex: 'description',
      align: 'left',
      width: 200,
      sorter: true,
    },
    {
      title: L('DisplayName:CreationTime'),
      dataIndex: 'creationTime',
      align: 'left',
      width: 150,
      sorter: true,
      format: (text) => {
        return formatToDateTime(text);
      },
    },
    {
      title: L('DisplayName:Webhooks'),
      dataIndex: 'webhooks',
      align: 'left',
      width: 200,
      sorter: true,
    },
  ];
}

