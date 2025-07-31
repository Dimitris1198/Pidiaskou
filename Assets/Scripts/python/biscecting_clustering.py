import sys
import json
import numpy as np

try:
    from sklearn.metrics import silhouette_score
    from sklearn.cluster import BisectingKMeans
except ImportError as e:
    print("An ImportError occurred.")
    print(f"Name: {e.name}")
    sys.exit()

def main_centroid(arr):
    center = np.mean(arr,axis=0)
    total_distance = 0
    for point in arr:
        total_distance +=np.linalg.norm(point - center)
    avg_dist = total_distance/len(arr)
    return {
        'center':center,
        "avg_dist":avg_dist
    }


def optimal_k(x,kmax):
    sil = []
    for k in range(2, kmax+1):
        kmeans = BisectingKMeans(n_clusters = k).fit(x)
        labels = kmeans.labels_
        sil.append({
            "k":k,
            "score":silhouette_score(x, labels, metric = 'euclidean')
            })
    return max(sil,key=lambda x:x['score'])

def main():
    try:
        received = input()
        parsed_data= json.loads(received)
        cords=[]
        for cord in parsed_data['positions']:
            cords.append([cord['x'],cord['y'],cord['z']])
        to_np = np.array(cords)
        centroid = main_centroid(to_np)
        if centroid['avg_dist']>0.3:
            optim_k = optimal_k(to_np,parsed_data['maxClusters'])
            kmeans = BisectingKMeans(n_clusters=optim_k['k']).fit(to_np)
            sys.stdout.write(json.dumps({"peaks":kmeans.cluster_centers_.tolist()}))
            return
        else:
            sys.stdout.write(json.dumps({"peaks":[centroid['center'].tolist()]}))

    except Exception as err:
        sys.stdout.write(json.dumps({"error":str(err)}))

main()